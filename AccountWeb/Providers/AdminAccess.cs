using DFCommonLib.Config;
using Microsoft.AspNetCore.Authorization;

namespace AccountWeb.Providers;

/// <summary>
/// Admin allowlist from <see cref="AccountWebSettings.AdminUsers"/>. Checked at sign-in and again on
/// every request, so removing a name from the config locks that user out without waiting for the
/// session cookie to expire.
/// </summary>
public static class AdminAccess
{
    /// <summary>Claim holding the login name the user signed in with (the name shown is the nickname).</summary>
    public const string UsernameClaim = "accountweb:username";

    public static IReadOnlyList<string> AdminUsers(IConfigurationHelper configuration) =>
        (configuration.Settings as AccountWebSettings)?.AdminUsers
            ?.Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .ToList() ?? [];

    // The account database compares usernames case-insensitively, so do the same here.
    public static bool IsAdmin(IConfigurationHelper configuration, string? username) =>
        !string.IsNullOrWhiteSpace(username) &&
        AdminUsers(configuration).Contains(username.Trim(), StringComparer.OrdinalIgnoreCase);
}

public sealed class AdminRequirement : IAuthorizationRequirement;

public sealed class AdminAuthorizationHandler(IConfigurationHelper configuration) : AuthorizationHandler<AdminRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
    {
        if (AdminAccess.IsAdmin(configuration, context.User.FindFirst(AdminAccess.UsernameClaim)?.Value))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
