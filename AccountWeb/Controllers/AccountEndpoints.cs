using AccountCommon.SharedModel;
using AccountWeb.Providers;
using DFCommonLib.Logger;

namespace AccountWeb.Controllers;

public sealed record CreateUserRequest(string? Username, string? Password, string? Nickname, string? Email);

/// <summary>Creates new DarkFactor accounts. Requires a signed-in AccountWeb user.</summary>
public static class AccountEndpoints
{
    public const int MaxFieldLength = 100;

    public static void Map(WebApplication app)
    {
        app.MapPost("/api/accounts", CreateAsync).RequireAuthorization();
        app.MapGet("/api/accounts", SearchAsync).RequireAuthorization();
        app.MapGet("/api/accounts/{id}", GetAsync).RequireAuthorization();
        app.MapPut("/api/accounts/{id}", UpdateAsync).RequireAuthorization();
        app.MapPut("/api/accounts/{id}/password", SetPasswordAsync).RequireAuthorization();
    }

    // What the browser gets for an account. Password and salt never leave the AccountServer.
    internal static object ToDto(AdminUserData user) => new
    {
        id = user.id,
        username = user.username,
        nickname = user.nickname,
        email = user.email,
        loginType = user.logintype switch { 1 => "DarkFactor", 2 => "GameCenter", _ => "Unknown" },
        created = user.created,
        lastLogin = user.lastlogin,
        numLogins = user.numlogins,
    };

    internal static async Task<IResult> SearchAsync(string? query, IAccountService accounts, CancellationToken cancellationToken)
    {
        if ((query?.Length ?? 0) > MaxFieldLength)
        {
            return Results.BadRequest(new { error = $"Search can be at most {MaxFieldLength} characters." });
        }

        var result = await accounts.SearchUsersAsync(query?.Trim(), cancellationToken);
        return result.Status == AccountResultStatus.Ok
            ? Results.Ok((result.Users ?? []).Select(ToDto))
            : ErrorResult(result);
    }

    internal static async Task<IResult> GetAsync(uint id, IAccountService accounts, CancellationToken cancellationToken)
    {
        var result = await accounts.GetUserAsync(id, cancellationToken);
        return result.Status == AccountResultStatus.Ok ? Results.Ok(ToDto(result.User!)) : ErrorResult(result);
    }

    public sealed record UpdateUserRequest(string? Nickname, string? Email);

    internal static async Task<IResult> UpdateAsync(
        uint id, UpdateUserRequest? request, IAccountService accounts, HttpContext http, CancellationToken cancellationToken)
    {
        var nickname = request?.Nickname?.Trim();
        var email = request?.Email?.Trim();
        if (string.IsNullOrEmpty(nickname))
        {
            return Results.BadRequest(new { error = "Nickname is required." });
        }

        if (nickname.Length > MaxFieldLength || (email?.Length ?? 0) > MaxFieldLength)
        {
            return Results.BadRequest(new { error = $"Fields can be at most {MaxFieldLength} characters." });
        }

        var result = await accounts.UpdateUserAsync(id, nickname, email, cancellationToken);
        if (result.Status != AccountResultStatus.Ok)
        {
            return ErrorResult(result);
        }

        DFLogger.LogOutput(DFLogLevel.INFO, "UpdateAccount", $"Account {id} updated by '{http.User.Identity?.Name}'.");
        return Results.Ok(ToDto(result.User!));
    }

    public sealed record SetPasswordRequest(string? Password);

    internal static async Task<IResult> SetPasswordAsync(
        uint id, SetPasswordRequest? request, IAccountService accounts, HttpContext http, CancellationToken cancellationToken)
    {
        var password = request?.Password;
        if (string.IsNullOrEmpty(password) || password.Length > MaxFieldLength)
        {
            return Results.BadRequest(new { error = "Password is required." });
        }

        var result = await accounts.SetPasswordAsync(id, password, cancellationToken);
        if (result.Status != AccountResultStatus.Ok)
        {
            return ErrorResult(result);
        }

        DFLogger.LogOutput(DFLogLevel.INFO, "SetPassword", $"Password for account {id} set by '{http.User.Identity?.Name}'.");
        return Results.NoContent();
    }

    internal static async Task<IResult> CreateAsync(
        CreateUserRequest? request, IAccountService accounts, HttpContext http, CancellationToken cancellationToken) =>
        (await CreateCoreAsync(request, accounts, $"'{http.User.Identity?.Name}'", cancellationToken)).Result;

    /// <summary>
    /// Validates the request and creates the account. <paramref name="createdBy"/> is only used for the
    /// log line. Returns the HTTP result and whether an account was created.
    /// </summary>
    internal static async Task<(IResult Result, bool Created)> CreateCoreAsync(
        CreateUserRequest? request, IAccountService accounts, string createdBy, CancellationToken cancellationToken)
    {
        var username = request?.Username?.Trim();
        var nickname = request?.Nickname?.Trim();
        var email = request?.Email?.Trim();
        var password = request?.Password;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(nickname) || string.IsNullOrEmpty(password))
        {
            return (Results.BadRequest(new { error = "Username, nickname and password are required." }), false);
        }

        if (username.Length > MaxFieldLength || nickname.Length > MaxFieldLength ||
            password.Length > MaxFieldLength || (email?.Length ?? 0) > MaxFieldLength)
        {
            return (Results.BadRequest(new { error = $"Fields can be at most {MaxFieldLength} characters." }), false);
        }

        var result = await accounts.CreateAccountAsync(username, password, nickname, email, cancellationToken);
        if (result.Status != AccountResultStatus.Ok)
        {
            return (ErrorResult(result), false);
        }

        DFLogger.LogOutput(
            DFLogLevel.INFO,
            "CreateAccount",
            $"Account {result.AccountId} '{username}' created by {createdBy}.");
        return (Results.Created(
            $"/api/accounts/{result.AccountId}",
            new { id = result.AccountId, username, nickname = result.Nickname }), true);
    }

    internal static IResult ErrorResult(AccountResult result) => result.Status switch
    {
        AccountResultStatus.InvalidCredentials => Results.Json(
            new { error = result.Error ?? "Invalid username or password." }, statusCode: StatusCodes.Status401Unauthorized),
        AccountResultStatus.InvalidInput => Results.BadRequest(new { error = result.Error ?? "Invalid input." }),
        AccountResultStatus.AlreadyExists => Results.Conflict(new { error = result.Error ?? "User already exists." }),
        AccountResultStatus.NotFound => Results.NotFound(new { error = result.Error ?? "User does not exist." }),
        AccountResultStatus.NotAllowed => Results.Json(
            new { error = "The account server does not allow AccountWeb to manage users. Add its client id to AdminClients in the AccountServer config." },
            statusCode: StatusCodes.Status502BadGateway),
        AccountResultStatus.NotConfigured => Results.Json(
            new { error = "The account server is not configured." }, statusCode: StatusCodes.Status503ServiceUnavailable),
        _ => Results.Json(new { error = "Unable to reach the account server." }, statusCode: StatusCodes.Status502BadGateway),
    };
}
