

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
        void SetExpiresWhen(uint expiresWhen);
        string GetClientId();
        string GetCode();
        string GetState();
        string GetToken();
        uint GetExpiresWhen();
    }

    public class ServerOAuth2SessionProvider : DFUserSession, IServerOAuth2SessionProvider
    {
        public static readonly string SessionClientIdKey = "SessionClientIdKey";
        public static readonly string SessionStateKey = "SessionStateIdKey";       
        public static readonly string SessionCodeKey = "SessionCodeKey";
        public static readonly string SessionTokenKey = "SessionTokenKey";
        public static readonly string SessionExpiresWhenKey = "SessionExpiresWhenKey";

        public ServerOAuth2SessionProvider(IHttpContextAccessor httpContext) : base("ServerOAuth2", httpContext)
        {
        }

        override public void RemoveSession()
        {
            RemoveConfig(SessionClientIdKey);
            RemoveConfig(SessionStateKey);
            RemoveConfig(SessionCodeKey);
            RemoveConfig(SessionTokenKey);
            RemoveConfig(SessionExpiresWhenKey);
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

        public uint GetExpiresWhen()
        {
            var expiresWhen = GetConfigInt(SessionExpiresWhenKey);
            return (uint)(expiresWhen ?? 0);
        }

        public void SetExpiresWhen(uint expiresWhen)
        {
            SetConfigInt(SessionExpiresWhenKey, (int)expiresWhen);
        }
    }
}