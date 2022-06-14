using Microsoft.AspNetCore.Http;
using DFCommonLib.Utils;

namespace AccountServer.Provider
{
    public interface IAccountSessionProvider : IDFUserSession
    {
        void SetAccountId(uint accountId);
        void SetAccountCode(string code);
        void SetAccountToken(string token);
        uint GetAccountId();
        string GetAccountCode();
        string GetAccountToken();
    }

    public class AccountSessionProvider : DFUserSession, IAccountSessionProvider
    {
        public static readonly string SessionAccountIdKey = "AccountId";
        public static readonly string SessionCodeKey = "Code";
        public static readonly string SessionTokenKey = "Token";

        public AccountSessionProvider( IHttpContextAccessor httpContext ) : base( "AccountServer", httpContext )
        {
        }

        override public void RemoveSession()
        {
            RemoveConfig(SessionAccountIdKey);
            RemoveConfig(SessionCodeKey);
            RemoveConfig(SessionTokenKey);
        }

        public void SetAccountId(uint accountId)
        {
            SetConfigInt(SessionAccountIdKey, (int)accountId);
        }

        public void SetAccountCode(string code)
        {
            SetConfigString(SessionCodeKey, code);
        }

        public void SetAccountToken(string token)
        {
            SetConfigString(SessionTokenKey, token);
        }

        public uint GetAccountId()
        {
            var accountId = GetConfigInt(SessionAccountIdKey);
            return (uint)(accountId ?? 0);
        }

        public string GetAccountCode()
        {
            return GetConfigString(SessionCodeKey);
        }

        public string GetAccountToken()
        {
            return GetConfigString(SessionTokenKey);
        }
    }
}