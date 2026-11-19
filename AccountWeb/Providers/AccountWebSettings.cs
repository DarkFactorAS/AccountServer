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
}
