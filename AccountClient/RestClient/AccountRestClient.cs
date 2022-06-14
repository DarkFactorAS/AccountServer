
using System.Threading.Tasks;
using DFCommonLib.HttpApi;
using DFCommonLib.Logger;
using AccountClientModule.Model;

namespace AccountClientModule.RestClient
{
    public interface IAccountRestClient : IDFRestClient
    {
        void SetEndpoint(string endpoint);
        Task<WebAPIData> LoginAccount(LoginData accountData);
        Task<WebAPIData> LoginToken(LoginTokenData accountTokenData);
        Task<WebAPIData> CreateAccount(CreateAccountData createAccountData);
        Task<WebAPIData> ResetPasswordWithEmail(string emailAddress);
        Task<WebAPIData> ResetPasswordWithCode(string code, string emailAddress);
        Task<WebAPIData> ResetPasswordWithToken(string token, string password);
    }

    public class AccountRestClient : DFRestClient, IAccountRestClient
    {
        private const int POST_CREATE = 1;
        private const int POST_LOGIN_WITH_USERNAME = 2;
        private const int POST_LOGIN_WITH_TOKEN = 3;
        private const int POST_RESETPASSWORD_WITH_EMAIL = 4;
        private const int POST_RESETPASSWORD_WITH_CODE = 5;
        private const int POST_RESETPASSWORD_WITH_TOKEN = 6;
        private string _endpoint;

        public AccountRestClient(IDFLogger<DFRestClient> logger ) : base(logger)
        {
        }

        override protected string GetHostname()
        {
            if ( _endpoint != null )
            {
                return _endpoint;
            }
            return "";
        }

        override protected string GetModule()
        {
            return "Account";
        }

        public void SetEndpoint(string endpoint)
        {
            _endpoint = endpoint;
        }

        public async Task<WebAPIData> LoginAccount(LoginData loginData)
        {
            var response = await PutJsonData(POST_LOGIN_WITH_USERNAME,"LoginAccount",loginData);
            return response;
        }

        public async Task<WebAPIData> LoginToken(LoginTokenData loginData)
        {
            var response = await PutJsonData(POST_LOGIN_WITH_TOKEN,"LoginToken",loginData);
            return response;
        }

        public async Task<WebAPIData> CreateAccount(CreateAccountData createAccountData)
        {
            var response = await PutJsonData(POST_CREATE,"CreateAccount",createAccountData);
            return response;
        }

        public async Task<WebAPIData> ResetPasswordWithEmail(string emailAddress)
        {
            var data = "{ \"emailAddress\":\"" + emailAddress + "\" }";
            var response = await PutJsonData(POST_RESETPASSWORD_WITH_EMAIL,"ResetPasswordWithEmail",data);
            return response;
        }

        public async Task<WebAPIData> ResetPasswordWithCode(string code, string emailAddress)
        {
            var data = "{ \"code\":\"" + code + "\", \"emailAddress\":\"" + emailAddress + "\" }";
            var response = await PutJsonData(POST_RESETPASSWORD_WITH_CODE,"ResetPasswordWithCode",data);
            return response;
        }

        public async Task<WebAPIData> ResetPasswordWithToken(string token, string password)
        {
            var data = "{ \"token\":\"" + token + "\", \"password\":\"" + password + "\" }";
            var response = await PutJsonData(POST_RESETPASSWORD_WITH_TOKEN,"ResetPasswordWithToken",data);
            return response;
        }
    }
}