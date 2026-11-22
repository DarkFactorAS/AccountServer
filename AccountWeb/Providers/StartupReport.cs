using DFCommonLib.Logger;

namespace AccountWeb.Providers;

/// <summary>
/// Logs at startup which requirements for first-run setup (creating the first admin) and for admin
/// login are fulfilled, so a misconfiguration shows up in the console instead of as a failed request.
/// Only the config is checked; whether the AccountServer is reachable and lists this client in
/// AdminClients is only known on the first call.
/// </summary>
public static class StartupReport
{
    private const string Section = "Startup";

    public static void Log(AccountWebSettings? settings, string environmentName)
    {
        var server = settings?.AccountServer;
        var endpoint = server?.Endpoint?.Trim();
        var clientId = server?.ClientId?.Trim();
        var admins = settings?.AdminUsers?
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .ToList() ?? [];
        var allowRemote = settings?.AllowRemoteSetup ?? false;

        // Set by the official .NET container images. Inside a container, loopback is the container
        // itself: browser requests arrive from the Docker gateway, and 127.0.0.1 is not the host.
        var inContainer = string.Equals(
            Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase);
        var endpointIsLoopback = Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri) && endpointUri.IsLoopback;

        var hasEndpoint = !string.IsNullOrEmpty(endpoint) && !(inContainer && endpointIsLoopback);
        var hasClientId = !string.IsNullOrEmpty(clientId);
        var hasSecret = !string.IsNullOrWhiteSpace(server?.ClientSecret);
        var serverOk = hasEndpoint && hasClientId && hasSecret;

        Line(true, $"Config file: Config/appsettings.{environmentName}.json");

        Line(null, "Account server (needed for both setup and login):");
        Check(hasEndpoint, "AccountServer.Endpoint", string.IsNullOrEmpty(endpoint)
            ? "not set"
            : hasEndpoint
                ? endpoint
                : $"{endpoint} points at this container, not the AccountServer; use its service name (e.g. http://accountserver:8080)");
        Check(hasClientId, "AccountServer.ClientId", hasClientId ? clientId! : "not set");
        Check(hasSecret, "AccountServer.ClientSecret", hasSecret ? "set" : "not set");
        Note($"The AccountServer must list client id '{(hasClientId ? clientId : "?")}' in AdminClients (default: AccountWeb); not checked until first call.");

        var setupOpen = admins.Count == 0;
        // In a container no browser request comes from loopback, so setup needs AllowRemoteSetup.
        var setupReachable = allowRemote || !inContainer;
        var setupOk = setupOpen && serverOk && setupReachable;
        var setupState = !setupOk ? "NOT AVAILABLE" : allowRemote ? "AVAILABLE from any address" : "AVAILABLE from this machine only";
        Line(setupOk, $"First-run setup (create first admin): {setupState}");
        Check(setupOpen, "AdminUsers is empty", setupOpen ? "yes" : $"no ({admins.Count} configured), setup is closed");
        if (setupOpen)
        {
            Check(setupReachable, "AllowRemoteSetup", allowRemote
                ? "true, setup is offered to ANY address; set it back to false after the first admin exists"
                : inContainer
                    ? "false, but running in a container where browser requests never come from loopback; set it to true for the first run"
                    : "false, setup is only offered to requests from this machine (loopback)");
        }

        if (!serverOk && setupOpen)
        {
            Note("Setup page is shown, but creating the account will fail until the account server settings above are fixed.");
        }

        var loginOk = admins.Count > 0 && serverOk;
        Line(loginOk, $"Admin login: {(loginOk ? "AVAILABLE" : "NOT AVAILABLE")}");
        Check(admins.Count > 0, "AdminUsers", admins.Count > 0 ? string.Join(", ", admins) : "empty, nobody can sign in");
        if (admins.Count > 0 && allowRemote)
        {
            Note("AllowRemoteSetup is still true; it has no effect now that AdminUsers is set, but should be turned off.");
        }
    }

    private static void Check(bool ok, string name, string detail) =>
        Line(ok, $"  [{(ok ? "OK" : "MISSING")}] {name}: {detail}");

    private static void Note(string message) => Line(null, $"  [NOTE] {message}");

    private static void Line(bool? ok, string message) =>
        DFLogger.LogOutput(ok == false ? DFLogLevel.WARNING : DFLogLevel.INFO, Section, message);
}
