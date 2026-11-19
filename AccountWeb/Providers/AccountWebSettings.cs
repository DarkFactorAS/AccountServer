using DFCommonLib.Config;

namespace AccountWeb.Providers;

/// <summary>Account server connection (same shape as DFWeb's and the AI dashboard's <c>AccountServer</c> section).</summary>
public sealed class AccountServerSettings
{
    public string? Endpoint { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? Scope { get; set; }
}

/// <summary>AccountWeb appsettings: the shared DarkFactor settings plus the account server.</summary>
public sealed class AccountWebSettings : AppSettings
{
    public AccountServerSettings? AccountServer { get; set; }

    /// <summary>
    /// Usernames (login names, case-insensitive) allowed to use AccountWeb. Everyone else is refused,
    /// even with a valid DarkFactor account. Empty means nobody can sign in.
    /// </summary>
    public List<string> AdminUsers { get; set; } = [];

    /// <summary>
    /// First-run setup (creating the first account while <see cref="AdminUsers"/> is empty) is only
    /// offered to requests from this machine. Set to true to allow it from other addresses, e.g. when
    /// AccountWeb runs in Docker (browser requests then arrive from the Docker gateway). Turn it off
    /// again once the first admin exists.
    /// </summary>
    public bool AllowRemoteSetup { get; set; }
}
