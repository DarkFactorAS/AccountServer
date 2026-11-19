using AccountClientModule.Client;
using DFCommonLib.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace AccountWeb.Providers;

/// <summary>
/// Cookie session for signed-in AccountWeb users. Every endpoint requires a session unless it is
/// marked AllowAnonymous (health, login, logout and the SPA shell). API calls without a session get
/// 401 instead of a redirect so the frontend can show the login page.
/// </summary>
public static class WebAuthentication
{
    public const string CookieName = "accountweb_auth";
    public static readonly TimeSpan SessionLifetime = TimeSpan.FromHours(12);

    public static IServiceCollection AddAccountWebAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = CookieName;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.ExpireTimeSpan = SessionLifetime;
                options.SlidingExpiration = true;
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });
        // Signed in AND on the admin list. Used both as the default policy (RequireAuthorization())
        // and the fallback policy (endpoints without any attribute).
        var adminPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new AdminRequirement())
            .Build();
        services.AddSingleton<IAuthorizationHandler, AdminAuthorizationHandler>();
        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(adminPolicy)
            .SetFallbackPolicy(adminPolicy);
        return services;
    }

    /// <summary>
    /// Registers the account client and <see cref="IAccountService"/>. The account client's REST client
    /// resolves its dependencies through the static DFServices provider and keeps its state in the
    /// ASP.NET session, so both are set up here.
    /// </summary>
    public static IServiceCollection AddAccountServerClient(this IServiceCollection services)
    {
        DFServices.Create(services);
        services.AddHttpContextAccessor();
        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.Cookie.Name = "accountweb_session";
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });
        AccountClient.SetupService(services);
        services.AddScoped<IAccountService, AccountService>();
        return services;
    }
}
