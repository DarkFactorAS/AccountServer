using System.Net;
using AccountWeb.Providers;
using DFCommonLib.Config;
using DFCommonLib.Logger;

namespace AccountWeb.Controllers;

/// <summary>
/// One-time state for first-run setup. After the first account is created, setup stays closed until
/// AccountWeb restarts (by then the new username should be in AdminUsers, which closes it for good).
/// </summary>
public sealed class SetupState
{
    public SemaphoreSlim Lock { get; } = new(1, 1);
    public string? CreatedUsername { get; set; }
}

/// <summary>
/// First-run setup: while AdminUsers is empty, lets someone on this machine (or anywhere, with
/// AllowRemoteSetup) create one account without signing in, to become the first admin.
/// </summary>
public static class SetupEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/setup/status", Status).AllowAnonymous();
        app.MapPost("/api/setup/first-account", CreateFirstAccountAsync).AllowAnonymous();
    }

    internal static bool IsSetupOpen(IConfigurationHelper configuration, SetupState state, HttpContext http)
    {
        if (AdminAccess.AdminUsers(configuration).Count > 0 || state.CreatedUsername is not null)
        {
            return false;
        }

        var allowRemote = (configuration.Settings as AccountWebSettings)?.AllowRemoteSetup ?? false;
        // No forwarded-headers middleware is used, so RemoteIpAddress is the real peer and cannot be spoofed by headers.
        var remote = http.Connection.RemoteIpAddress;
        return allowRemote || (remote is not null && IPAddress.IsLoopback(remote));
    }

    internal static IResult Status(
        IConfigurationHelper configuration, SetupState state, HttpContext http, IWebHostEnvironment environment)
    {
        if (IsSetupOpen(configuration, state, http))
        {
            return Results.Ok(new
            {
                setupMode = true,
                configFile = $"Config/appsettings.{environment.EnvironmentName}.json",
            });
        }

        return Results.Ok(new { setupMode = false });
    }

    internal static async Task<IResult> CreateFirstAccountAsync(
        CreateUserRequest? request,
        IAccountService accounts,
        IConfigurationHelper configuration,
        SetupState state,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        // Serialize so two simultaneous requests cannot both create a "first" account.
        await state.Lock.WaitAsync(cancellationToken);
        try
        {
            if (!IsSetupOpen(configuration, state, http))
            {
                return Results.Json(
                    new { error = "First-run setup is not available." },
                    statusCode: StatusCodes.Status403Forbidden);
            }

            var (result, created) = await AccountEndpoints.CreateCoreAsync(
                request, accounts, $"first-run setup from {http.Connection.RemoteIpAddress}", cancellationToken);
            if (created)
            {
                state.CreatedUsername = request!.Username!.Trim();
                DFLogger.LogOutput(
                    DFLogLevel.WARNING,
                    "Setup",
                    $"First account '{state.CreatedUsername}' created. Add it to AdminUsers and restart AccountWeb.");
            }

            return result;
        }
        finally
        {
            state.Lock.Release();
        }
    }
}
