

namespace AccountCommon.SharedModel
{
    public class OAuth2VerifyTokenData
    {
        public string? Token { get; set; }

        public OAuth2VerifyTokenData(string token)
        {
            Token = token;
        }
    }
}