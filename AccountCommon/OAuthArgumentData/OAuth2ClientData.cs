

namespace AccountCommon.SharedModel
{
    public class OAuth2ClientData
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
        public string Scope { get; set; }
        public string State { get; set; }

        public OAuth2ClientData()
        {
        }

        public OAuth2ClientData(string clientId, string clientSecret, string redirectUri, string scope, string state)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            RedirectUri = redirectUri;
            Scope = scope;
            State = state;
        }
    }
}