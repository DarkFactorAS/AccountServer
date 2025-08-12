

using DFCommonLib.Config;

namespace AccountTestClient
{
    public class AccountServer
    {
        public string? BaseUrl { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
    }

    public class TestAccountConfig : AppSettings
    {
        public AccountServer? AccountServer { get; set; }
    }
}