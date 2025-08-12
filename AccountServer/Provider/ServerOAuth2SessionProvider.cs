

using DFCommonLib.Utils;
using Microsoft.AspNetCore.Http;

namespace AccountServer.Provider
{
    public interface IServerOAuth2SessionProvider : IDFUserSession
    {
        void SetClientId(string clientId);
        void SetClientSecret(string clientSecret);
        void SetCode(string code);
        void SetState(string state);
        string GetClientId();
        string GetClientSecret();
        string GetCode();
        string GetState();
    }

    public class ServerOAuth2SessionProvider : DFUserSession, IServerOAuth2SessionProvider
    {
        // TODO: Implement session management for OAuth2

        public ServerOAuth2SessionProvider(IHttpContextAccessor httpContext) : base("ServerOAuth2", httpContext)
        {
        }

        public void SetClientId(string clientId)
        {
            SetConfigString("ClientId", clientId);
        }

        public void SetClientSecret(string clientSecret)
        {
            SetConfigString("ClientSecret", clientSecret);
        }

        public string GetClientId()
        {
            return GetConfigString("ClientId");
        }

        public string GetClientSecret()
        {
            return GetConfigString("ClientSecret");
        }

        public void SetCode(string code)
        {
            SetConfigString("Code", code);
        }

        public string GetCode()
        {
            return GetConfigString("Code");
        }

        public void SetState(string state)
        {
            SetConfigString("State", state);
        }

        public string GetState()
        {
            return GetConfigString("State");
        }
    }
}