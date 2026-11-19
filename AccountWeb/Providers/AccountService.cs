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
    NotConfigured,
    Unavailable,
}

public sealed record AccountResult(
    AccountResultStatus Status,
    string? Nickname = null,
    uint AccountId = 0,
    string? Error = null);

public interface IAccountService
{
    /// <summary>Checks the credentials of an existing account (used to sign in to AccountWeb).</summary>
    Task<AccountResult> LoginAsync(string username, string password, CancellationToken cancellationToken);

    /// <summary>Creates a new DarkFactor account in the account database.</summary>
    Task<AccountResult> CreateAccountAsync(
        string username, string password, string nickname, string? email, CancellationToken cancellationToken);
}

/// <summary>
/// Talks to the AccountServer through the AccountClient, so new users get the same validation,
/// salting and hashing as every other account. Passwords are encrypted with
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
                // Same answer for unknown user and wrong password.
                AccountData.ErrorCode.UserDoesNotExist or AccountData.ErrorCode.WrongPassword =>
                    new AccountResult(AccountResultStatus.InvalidCredentials, Error: "Invalid username or password."),
                // Anything else (including transport errors the client maps to an error code) is a server problem.
                _ => new AccountResult(AccountResultStatus.Unavailable),
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
            data => data.errorCode switch
            {
                AccountData.ErrorCode.OK => new AccountResult(AccountResultStatus.Ok, data.nickname ?? nickname, data.id),
                AccountData.ErrorCode.UserAlreadyExist or AccountData.ErrorCode.NicknameAlreadyExist =>
                    new AccountResult(AccountResultStatus.AlreadyExists, Error: data.errorMessage),
                AccountData.ErrorCode.ErrorInData or
                AccountData.ErrorCode.InvalidUsername or
                AccountData.ErrorCode.UsernameInvalidCharacters or
                AccountData.ErrorCode.NicknameInvalidCharacters or
                AccountData.ErrorCode.PasswordInvalidLength or
                AccountData.ErrorCode.PasswordInvalidCharacters or
                AccountData.ErrorCode.NotValidEmail or
                AccountData.ErrorCode.NicknameAndUsernameEqual =>
                    new AccountResult(AccountResultStatus.InvalidInput, Error: data.errorMessage),
                _ => new AccountResult(AccountResultStatus.Unavailable, Error: data.errorMessage),
            },
            cancellationToken);

    private async Task<AccountResult> CallAsync(
        string operation,
        Func<IAccountClient, AccountData?> call,
        Func<AccountData, AccountResult> map,
        CancellationToken cancellationToken)
    {
        var server = (configuration.Settings as AccountWebSettings)?.AccountServer;
        if (server is null || string.IsNullOrWhiteSpace(server.Endpoint))
        {
            DFLogger.LogOutput(DFLogLevel.ERROR, operation, "AccountServer.Endpoint is not configured.");
            return new AccountResult(AccountResultStatus.NotConfigured);
        }

        AccountData? data;
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
