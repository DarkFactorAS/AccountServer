

using DFCommonLib.Utils;
using Microsoft.AspNetCore.Http;

namespace AccountServer.Provider
{
    public interface IServerOAuth2SessionProvider : IDFUserSession
    {
        void SetClientId(string clientId);
        void SetCode(string code);
        void SetState(string state);
        void SetToken(string token);
        void SetScope(string scope);
        void SetIssuer(string issuer);

        void SetServerSecret(string serverSecret);
        string GetClientId();
        string GetCode();
        string GetState();
        string GetToken();
        string GetScope();
        string GetServerSecret();
        string GetIssuer();
    }

    public class ServerOAuth2SessionProvider : DFUserSession, IServerOAuth2SessionProvider
    {
        public static readonly string SessionClientIdKey = "SessionClientIdKey";
        public static readonly string SessionStateKey = "SessionStateIdKey";
        public static readonly string SessionCodeKey = "SessionCodeKey";
        public static readonly string SessionTokenKey = "SessionTokenKey";
        public static readonly string SessionScopeKey = "SessionScopeKey";
        public static readonly string SessionServerSecretKey = "SessionServerSecretKey";
        public static readonly string SessionIssuerKey = "SessionIssuerKey";

        public ServerOAuth2SessionProvider(IHttpContextAccessor httpContext) : base("ServerOAuth2", httpContext)
        {
        }

        override public void RemoveSession()
        {
            RemoveConfig(SessionClientIdKey);
            RemoveConfig(SessionStateKey);
            RemoveConfig(SessionCodeKey);
            RemoveConfig(SessionTokenKey);
            RemoveConfig(SessionScopeKey);
            RemoveConfig(SessionServerSecretKey);
            RemoveConfig(SessionIssuerKey);
        }


        public void SetClientId(string clientId)
        {
            SetConfigString(SessionClientIdKey, clientId);
        }

        public string GetClientId()
        {
            return GetConfigString(SessionClientIdKey);
        }

        public void SetCode(string code)
        {
            SetConfigString(SessionCodeKey, code);
        }

        public string GetCode()
        {
            return GetConfigString(SessionCodeKey);
        }

        public void SetState(string state)
        {
            SetConfigString(SessionStateKey, state);
        }

        public string GetState()
        {
            return GetConfigString(SessionStateKey);
        }

        public string GetToken()
        {
            return GetConfigString(SessionTokenKey);
        }

        public void SetToken(string token)
        {
            SetConfigString(SessionTokenKey, token);
        }

        public void SetScope(string scope)
        {
            SetConfigString(SessionScopeKey, scope);
        }

        public string GetScope()
        {
            return GetConfigString(SessionScopeKey);
        }

        public void SetServerSecret(string serverSecret)
        {
            SetConfigString(SessionServerSecretKey, serverSecret);
        }

        public string GetServerSecret()
        {
            return GetConfigString(SessionServerSecretKey);
        }

        public void SetIssuer(string issuer)
        {
            SetConfigString(SessionIssuerKey, issuer);
        }

        public string GetIssuer()
        {
            return GetConfigString(SessionIssuerKey);
        }
    }
}