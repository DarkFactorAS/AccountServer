using System.Security.Claims;
using AccountWeb.Providers;
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
        LoginRequest? request, IAccountService accounts, HttpContext http, CancellationToken cancellationToken)
    {
        var username = request?.Username?.Trim();
        var password = request?.Password;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) ||
            username.Length > MaxUsernameLength || password.Length > MaxPasswordLength)
        {
            return Results.BadRequest(new { error = "Username and password are required." });
        }

        var result = await accounts.LoginAsync(username, password, cancellationToken);
        if (result.Status != AccountResultStatus.Ok)
        {
            return AccountEndpoints.ErrorResult(result);
        }

        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, result.Nickname ?? username),
                new Claim(ClaimTypes.NameIdentifier, result.AccountId.ToString()),
            ],
            CookieAuthenticationDefaults.AuthenticationScheme);
        await http.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });
        return Results.Ok(new { username = identity.Name });
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
