
using DFCommonLib.HttpApi;

namespace AccountCommon.SharedModel
{
    public class OAuth2CodeResponse : WebAPIData
    {
        public string? AccessToken { get; set; }
        public string? State { get; set; }
        public string? TokenType { get; set; }
        public uint ExpiresIn { get; set; }
    }
}