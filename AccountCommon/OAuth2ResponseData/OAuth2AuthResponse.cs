
using DFCommonLib.HttpApi;

namespace AccountCommon.SharedModel
{
    public class OAuth2AuthResponse : WebAPIData
    {
        public string? Code { get; set; }
        public string? State { get; set; }
    }
}