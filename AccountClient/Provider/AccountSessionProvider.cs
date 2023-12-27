using Microsoft.AspNetCore.Http;

namespace AccountClientModule.Provider
{
    public interface IAccountSessionProvider
    {
        void RemoveSession();
        string GetToken();
        void SetToken(string token);
        string GetEmail();
        void SetEmail(string email);
    }

    public class AccountSessionProvider : DFSessionProvider, IAccountSessionProvider
    {
        public static readonly string SessionEmail = "Email";
        public static readonly string SessionTokenKey = "Token";

        public AccountSessionProvider( IHttpContextAccessor httpContext ) : base("AccountSessionProvider",httpContext)
        {
        }

        public void RemoveSession()
        {
            RemoveConfig(SessionTokenKey);
            RemoveConfig(SessionEmail);
        }

        public string GetEmail()
        {
            return GetConfigString(SessionEmail);
        }

        public void SetEmail(string email)
        {
            SetConfigString(SessionEmail, email);
        }

        public string GetToken()
        {
            return GetConfigString(SessionTokenKey);
        }

        public void SetToken(string token)
        {
            SetConfigString(SessionTokenKey, token);
        }
    }
}
