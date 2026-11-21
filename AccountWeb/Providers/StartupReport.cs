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

        var hasEndpoint = !string.IsNullOrEmpty(endpoint);
        var hasClientId = !string.IsNullOrEmpty(clientId);
        var hasSecret = !string.IsNullOrWhiteSpace(server?.ClientSecret);
        var serverOk = hasEndpoint && hasClientId && hasSecret;

        Line(true, $"Config file: Config/appsettings.{environmentName}.json");

        Line(null, "Account server (needed for both setup and login):");
        Check(hasEndpoint, "AccountServer.Endpoint", hasEndpoint ? endpoint! : "not set");
        Check(hasClientId, "AccountServer.ClientId", hasClientId ? clientId! : "not set");
        Check(hasSecret, "AccountServer.ClientSecret", hasSecret ? "set" : "not set");
        Note($"The AccountServer must list client id '{(hasClientId ? clientId : "?")}' in AdminClients (default: AccountWeb); not checked until first call.");

        var setupOpen = admins.Count == 0;
        Line(setupOpen && serverOk, $"First-run setup (create first admin): {(setupOpen && serverOk ? "AVAILABLE" : "NOT AVAILABLE")}");
        Check(setupOpen, "AdminUsers is empty", setupOpen ? "yes" : $"no ({admins.Count} configured), setup is closed");
        if (setupOpen)
        {
            Check(true, "AllowRemoteSetup", allowRemote
                ? "true, setup is offered to ANY address; set it back to false after the first admin exists"
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
