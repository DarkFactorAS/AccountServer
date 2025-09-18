
using System.Threading.Tasks;
using DFCommonLib.HttpApi;
using DFCommonLib.Logger;
using AccountCommon.SharedModel;
using DFCommonLib.HttpApi.OAuth2;

namespace AccountClientModule.RestClient
{
    public interface IAccountRestClient : IDFOAuth2RestClient
    {
        Task<WebAPIData> LoginAccount(LoginData accountData);
        Task<WebAPIData> LoginToken(LoginTokenData accountTokenData);
        Task<WebAPIData> LoginGameCenter(LoginGameCenterData accountData);
        Task<WebAPIData> CreateAccount(CreateAccountData createAccountData);
        Task<WebAPIData> ResetPasswordWithEmail(string emailAddress);
        Task<WebAPIData> ResetPasswordWithCode(string code, string emailAddress);
        Task<WebAPIData> ResetPasswordWithToken(string token, string password);
    }

    public class AccountRestClient : DFOAuth2RestClient, IAccountRestClient
    {
        private const int POST_CREATE = 1;
        private const int POST_LOGIN_WITH_USERNAME = 2;
        private const int POST_LOGIN_WITH_TOKEN = 3;
        private const int POST_RESETPASSWORD_WITH_EMAIL = 4;
        private const int POST_RESETPASSWORD_WITH_CODE = 5;
        private const int POST_RESETPASSWORD_WITH_TOKEN = 6;
        private const int POST_LOGIN_WITH_GAMECENTER = 7;

        public AccountRestClient() : base()
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

        public async Task<WebAPIData> LoginAccount(LoginData loginData)
        {
            await AuthenticateIfNeeded();
            var response = await PutData(POST_LOGIN_WITH_USERNAME,"LoginAccount",loginData);
            return response;
        }

        public async Task<WebAPIData> LoginToken(LoginTokenData loginData)
        {
            await AuthenticateIfNeeded();
            var response = await PutData(POST_LOGIN_WITH_TOKEN,"LoginToken",loginData);
            return response;
        }

        public async Task<WebAPIData> LoginGameCenter(LoginGameCenterData loginData)
        {
            await AuthenticateIfNeeded();
            var response = await PutData(POST_LOGIN_WITH_GAMECENTER,"LoginGameCenter",loginData);
            return response;
        }

        public async Task<WebAPIData> CreateAccount(CreateAccountData createAccountData)
        {
            await AuthenticateIfNeeded();
            var response = await PutData(POST_CREATE, "CreateAccount", createAccountData);
            return response;
        }

        public async Task<WebAPIData> ResetPasswordWithEmail(string emailAddress)
        {
            await AuthenticateIfNeeded();
            var data = new ResetPasswordDataEmail { emailAddress = emailAddress };
            var response = await PutData(POST_RESETPASSWORD_WITH_EMAIL,"ResetPasswordWithEmail",data);
            return response;
        }

        public async Task<WebAPIData> ResetPasswordWithCode(string code, string emailAddress)
        {            
            await AuthenticateIfNeeded();
            var data = new ResetPasswordDataCode
            {
                code = code,
                emailAddress = emailAddress
            };
            var response = await PutData(POST_RESETPASSWORD_WITH_CODE,"ResetPasswordWithCode",data);
            return response;
        }

        public async Task<WebAPIData> ResetPasswordWithToken(string token, string password)
        {
            await AuthenticateIfNeeded();
            var data = new ResetPasswordDataToken
            {
                token = token,
                password = password
            };

            var response = await PutData(POST_RESETPASSWORD_WITH_TOKEN,"ResetPasswordWithToken",data);
            return response;
        }
    }
}