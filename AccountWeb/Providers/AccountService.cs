using AccountClientModule.Client;
using AccountCommon.SharedModel;
using DFCommonLib.Config;
using DFCommonLib.Logger;
using DFCommonLib.Utils;

namespace AccountWeb.Providers;

public enum AccountResultStatus
{
    Ok,
    InvalidCredentials,
    InvalidInput,
    AlreadyExists,
    NotFound,
    /// <summary>The AccountServer refuses admin calls from this OAuth2 client (AdminClients).</summary>
    NotAllowed,
    NotConfigured,
    Unavailable,
}

public sealed record AccountResult(
    AccountResultStatus Status,
    string? Nickname = null,
    uint AccountId = 0,
    string? Error = null,
    AdminUserData? User = null,
    IReadOnlyList<AdminUserData>? Users = null,
    /// <summary>Why the call failed, for the server log only. Never sent to the browser.</summary>
    string? LogReason = null);

public interface IAccountService
{
    /// <summary>Checks the credentials of an existing account (used to sign in to AccountWeb).</summary>
    Task<AccountResult> LoginAsync(string username, string password, CancellationToken cancellationToken);

    /// <summary>Creates a new DarkFactor account in the account database.</summary>
    Task<AccountResult> CreateAccountAsync(
        string username, string password, string nickname, string? email, CancellationToken cancellationToken);

    /// <summary>Finds accounts by username, nickname or email (newest first, at most 50).</summary>
    Task<AccountResult> SearchUsersAsync(string? query, CancellationToken cancellationToken);

    Task<AccountResult> GetUserAsync(uint accountId, CancellationToken cancellationToken);

    Task<AccountResult> UpdateUserAsync(uint accountId, string nickname, string? email, CancellationToken cancellationToken);

    Task<AccountResult> SetPasswordAsync(uint accountId, string password, CancellationToken cancellationToken);
}

/// <summary>
/// Talks to the AccountServer through the AccountClient, so accounts get the same validation,
/// salting and hashing as everywhere else. Passwords are encrypted with
/// <see cref="DFCrypt.EncryptInput"/> before they leave this process and are never logged or stored.
/// </summary>
public sealed class AccountService(IAccountClient accountClient, IConfigurationHelper configuration) : IAccountService
{
    public Task<AccountResult> LoginAsync(string username, string password, CancellationToken cancellationToken) =>
        CallAsync(
            "Login",
            client => client.LoginAccount(new LoginData { username = username, password = DFCrypt.EncryptInput(password) }),
            data => data.errorCode switch
            {
                AccountData.ErrorCode.OK => new AccountResult(
                    AccountResultStatus.Ok, string.IsNullOrWhiteSpace(data.nickname) ? username : data.nickname, data.id),
                // Same answer to the browser for unknown user and wrong password; only the log tells them apart.
                AccountData.ErrorCode.UserDoesNotExist => new AccountResult(
                    AccountResultStatus.InvalidCredentials, Error: "Invalid username or password.", LogReason: "user does not exist"),
                AccountData.ErrorCode.WrongPassword => new AccountResult(
                    AccountResultStatus.InvalidCredentials, Error: "Invalid username or password.", LogReason: "wrong password"),
                // Anything else (including transport errors the client maps to an error code) is a server problem.
                _ => new AccountResult(AccountResultStatus.Unavailable, LogReason: $"account server returned {data.errorCode}"),
            },
            cancellationToken);

    public Task<AccountResult> CreateAccountAsync(
        string username, string password, string nickname, string? email, CancellationToken cancellationToken) =>
        CallAsync(
            "CreateAccount",
            client => client.CreateAccount(new CreateAccountData
            {
                username = username,
                password = DFCrypt.EncryptInput(password),
                nickname = nickname,
                email = email ?? string.Empty,
            }),
            data => data.errorCode == AccountData.ErrorCode.OK
                ? new AccountResult(AccountResultStatus.Ok, data.nickname ?? nickname, data.id)
                : Failure(data.errorCode, data.errorMessage),
            cancellationToken);

    public Task<AccountResult> SearchUsersAsync(string? query, CancellationToken cancellationToken) =>
        CallAsync(
            "AdminSearchUsers",
            client => client.AdminSearchUsers(query ?? string.Empty),
            data => data.errorCode == AccountData.ErrorCode.OK
                ? new AccountResult(AccountResultStatus.Ok, Users: data.users ?? [])
                : Failure(data.errorCode, data.errorMessage),
            cancellationToken);

    public Task<AccountResult> GetUserAsync(uint accountId, CancellationToken cancellationToken) =>
        CallAsync("AdminGetUser", client => client.AdminGetUser(accountId), UserResult, cancellationToken);

    public Task<AccountResult> UpdateUserAsync(
        uint accountId, string nickname, string? email, CancellationToken cancellationToken) =>
        CallAsync(
            "AdminUpdateUser",
            client => client.AdminUpdateUser(new AdminUpdateUserData { id = accountId, nickname = nickname, email = email ?? string.Empty }),
            UserResult,
            cancellationToken);

    public Task<AccountResult> SetPasswordAsync(uint accountId, string password, CancellationToken cancellationToken) =>
        CallAsync(
            "AdminSetPassword",
            client => client.AdminSetPassword(accountId, DFCrypt.EncryptInput(password)),
            UserResult,
            cancellationToken);

    private static AccountResult UserResult(AdminUserData data) =>
        data.errorCode == AccountData.ErrorCode.OK
            ? new AccountResult(AccountResultStatus.Ok, data.nickname, data.id, User: data)
            : Failure(data.errorCode, data.errorMessage);

    private static AccountResult Failure(AccountData.ErrorCode code, string? message) => code switch
    {
        AccountData.ErrorCode.UserDoesNotExist => new AccountResult(AccountResultStatus.NotFound, Error: message),
        AccountData.ErrorCode.UserAlreadyExist or AccountData.ErrorCode.NicknameAlreadyExist =>
            new AccountResult(AccountResultStatus.AlreadyExists, Error: message),
        AccountData.ErrorCode.ErrorInData or
        AccountData.ErrorCode.InvalidUsername or
        AccountData.ErrorCode.UsernameInvalidCharacters or
        AccountData.ErrorCode.NicknameInvalidCharacters or
        AccountData.ErrorCode.PasswordInvalidLength or
        AccountData.ErrorCode.PasswordInvalidCharacters or
        AccountData.ErrorCode.NotValidEmail or
        AccountData.ErrorCode.NicknameAndUsernameEqual =>
            new AccountResult(AccountResultStatus.InvalidInput, Error: message),
        AccountData.ErrorCode.NotAuthorized => new AccountResult(AccountResultStatus.NotAllowed, Error: message),
        _ => new AccountResult(AccountResultStatus.Unavailable, Error: message),
    };

    private async Task<AccountResult> CallAsync<T>(
        string operation,
        Func<IAccountClient, T?> call,
        Func<T, AccountResult> map,
        CancellationToken cancellationToken)
        where T : class
    {
        var server = (configuration.Settings as AccountWebSettings)?.AccountServer;
        if (server is null || string.IsNullOrWhiteSpace(server.Endpoint))
        {
            DFLogger.LogOutput(DFLogLevel.ERROR, operation, "AccountServer.Endpoint is not configured.");
            return new AccountResult(AccountResultStatus.NotConfigured);
        }

        T? data;
        try
        {
            accountClient.SetEndpoint(server.Endpoint);
            accountClient.SetAuthCredentials(server.ClientId ?? "", server.ClientSecret ?? "", server.Scope ?? "");
            // The account client API is synchronous; keep it off the request thread.
            data = await Task.Run(() => call(accountClient), cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            DFLogger.LogOutput(DFLogLevel.ERROR, operation, $"Account server call failed: {ex.GetType().Name}");
            return new AccountResult(AccountResultStatus.Unavailable);
        }

        return data is null ? new AccountResult(AccountResultStatus.Unavailable) : map(data);
    }
}
