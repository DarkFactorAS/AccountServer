using System.Security.Claims;
using AccountWeb.Providers;
using DFCommonLib.Config;
using DFCommonLib.Logger;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;

namespace AccountWeb.Controllers;

public sealed record LoginRequest(string? Username, string? Password);

/// <summary>Login, logout and current-user endpoints for the AccountWeb cookie session.</summary>
public static class AuthEndpoints
{
    public const int MaxUsernameLength = 100;
    public const int MaxPasswordLength = 1024;

    public static void Map(WebApplication app)
    {
        app.MapPost("/api/auth/login", LoginAsync).AllowAnonymous();
        app.MapPost("/api/auth/logout", (Delegate)LogoutAsync).AllowAnonymous();
        app.MapGet("/api/auth/me", Me).RequireAuthorization();
    }

    internal static async Task<IResult> LoginAsync(
        LoginRequest? request,
        IAccountService accounts,
        IConfigurationHelper configuration,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var username = request?.Username?.Trim();
        var password = request?.Password;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) ||
            username.Length > MaxUsernameLength || password.Length > MaxPasswordLength)
        {
            LogFailure(http, username, "username or password missing or too long");
            return Results.BadRequest(new { error = "Username and password are required." });
        }

        if (AdminAccess.AdminUsers(configuration).Count == 0)
        {
            LogFailure(http, username, "AdminUsers is empty; nobody can sign in");
            return Results.Json(
                new { error = "No admin users are configured on the server." },
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        // Check the password first, so the answer does not reveal which usernames are admins.
        var result = await accounts.LoginAsync(username, password, cancellationToken);
        if (result.Status != AccountResultStatus.Ok)
        {
            LogFailure(http, username, result.LogReason ?? result.Status.ToString());
            return AccountEndpoints.ErrorResult(result);
        }

        if (!AdminAccess.IsAdmin(configuration, username))
        {
            LogFailure(http, username, $"account {result.AccountId} is not in AdminUsers");
            return Results.Json(
                new { error = "This account is not allowed to use AccountWeb." },
                statusCode: StatusCodes.Status403Forbidden);
        }

        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, result.Nickname ?? username),
                new Claim(ClaimTypes.NameIdentifier, result.AccountId.ToString()),
                new Claim(AdminAccess.UsernameClaim, username),
            ],
            CookieAuthenticationDefaults.AuthenticationScheme);
        await http.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });
        DFLogger.LogOutput(
            DFLogLevel.INFO, "Login", $"'{ForLog(username)}' (account {result.AccountId}) signed in from {http.Connection.RemoteIpAddress}.");
        return Results.Ok(new { username = identity.Name });
    }

    // The password is never logged, not even its length.
    private static void LogFailure(HttpContext http, string? username, string reason) =>
        DFLogger.LogOutput(
            DFLogLevel.WARNING,
            "Login",
            $"Login failed for '{ForLog(username)}' from {http.Connection.RemoteIpAddress}: {reason}.");

    // The username is user input: drop control characters so it cannot forge extra log lines.
    private static string ForLog(string? username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return "";
        }

        var clean = new string(username.Where(c => !char.IsControl(c)).ToArray());
        return clean.Length > MaxUsernameLength ? clean[..MaxUsernameLength] + "..." : clean;
    }

    internal static async Task<IResult> LogoutAsync(HttpContext http)
    {
        await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        // The account client keeps its state in the ASP.NET session; drop it too.
        http.Features.Get<ISessionFeature>()?.Session.Clear();
        return Results.NoContent();
    }

    internal static IResult Me(ClaimsPrincipal user) => Results.Ok(new { username = user.Identity?.Name });
}
