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
    }

    internal static async Task<IResult> CreateAsync(
        CreateUserRequest? request, IAccountService accounts, HttpContext http, CancellationToken cancellationToken)
    {
        var username = request?.Username?.Trim();
        var nickname = request?.Nickname?.Trim();
        var email = request?.Email?.Trim();
        var password = request?.Password;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(nickname) || string.IsNullOrEmpty(password))
        {
            return Results.BadRequest(new { error = "Username, nickname and password are required." });
        }

        if (username.Length > MaxFieldLength || nickname.Length > MaxFieldLength ||
            password.Length > MaxFieldLength || (email?.Length ?? 0) > MaxFieldLength)
        {
            return Results.BadRequest(new { error = $"Fields can be at most {MaxFieldLength} characters." });
        }

        var result = await accounts.CreateAccountAsync(username, password, nickname, email, cancellationToken);
        if (result.Status != AccountResultStatus.Ok)
        {
            return ErrorResult(result);
        }

        DFLogger.LogOutput(
            DFLogLevel.INFO,
            "CreateAccount",
            $"Account {result.AccountId} '{username}' created by '{http.User.Identity?.Name}'.");
        return Results.Created($"/api/accounts/{result.AccountId}", new { id = result.AccountId, username, nickname = result.Nickname });
    }

    internal static IResult ErrorResult(AccountResult result) => result.Status switch
    {
        AccountResultStatus.InvalidCredentials => Results.Json(
            new { error = result.Error ?? "Invalid username or password." }, statusCode: StatusCodes.Status401Unauthorized),
        AccountResultStatus.InvalidInput => Results.BadRequest(new { error = result.Error ?? "Invalid input." }),
        AccountResultStatus.AlreadyExists => Results.Conflict(new { error = result.Error ?? "User already exists." }),
        AccountResultStatus.NotConfigured => Results.Json(
            new { error = "The account server is not configured." }, statusCode: StatusCodes.Status503ServiceUnavailable),
        _ => Results.Json(new { error = "Unable to reach the account server." }, statusCode: StatusCodes.Status502BadGateway),
    };
}
