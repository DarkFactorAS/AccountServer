
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using DFCommonLib.HttpApi;
using Newtonsoft.Json;

using AccountCommon.SharedModel;
using AccountClientModule.RestClient;
using AccountClientModule.Provider;

namespace AccountClientModule.Client
{
    public interface IAccountClient : IDFClient
    {
        AccountData LoginAccount(LoginData accountData);
        AccountData LoginToken(LoginTokenData accountData);
        AccountData LoginGameCenter(LoginGameCenterData accountData);
        AccountData CreateAccount(CreateAccountData createAccountData);
        ReturnData ResetPasswordWithEmail(string emailAddress);
        ReturnData ResetPasswordWithCode(string code, string verifyEmailAddress);
        ReturnData ResetPasswordWithToken(string password );
    }

    public class AccountClient : DFClient, IAccountClient
    {
        IAccountRestClient _restClient;
        IAccountSessionProvider _sessionProvider;

        public AccountClient(IAccountRestClient restClient, IAccountSessionProvider sessionProvider) : base(restClient)
        {
            _restClient = restClient;
            _sessionProvider = sessionProvider;
        }

        public AccountData LoginAccount(LoginData loginData)
        {
            var result = Task.Run(async() => await _restClient.LoginAccount(loginData)).Result;
            return ConvertFromRestData( result );
        }
    
        public AccountData LoginToken(LoginTokenData loginData)
        {
            var result = Task.Run(async() => await _restClient.LoginToken(loginData)).Result;
            return ConvertFromRestData( result );
        }

        public AccountData LoginGameCenter(LoginGameCenterData loginData)
        {
            if (loginData == null)
            {
                return new AccountData
                {
                    errorCode = AccountData.ErrorCode.ErrorInData,
                    errorMessage = "Login data cannot be null."
                };
            }

            if (string.IsNullOrWhiteSpace(loginData.nickname) || string.IsNullOrWhiteSpace(loginData.username) || string.IsNullOrWhiteSpace(loginData.password))
            {
                return new AccountData
                {
                    errorCode = AccountData.ErrorCode.ErrorInData,
                    errorMessage = "Nickname, username and password are required."
                };
            }

            var result = Task.Run(async() => await _restClient.LoginGameCenter(loginData)).Result;
            return ConvertFromRestData( result );
        }

        public AccountData CreateAccount(CreateAccountData createAccountData)
        {
            var result = Task.Run(async() => await _restClient.CreateAccount(createAccountData)).Result;
            return ConvertFromRestData( result );
        }

        public ReturnData ResetPasswordWithEmail(string emailAddress)
        {
            _sessionProvider.RemoveSession();

            var result = Task.Run(async() => await _restClient.ResetPasswordWithEmail(emailAddress)).Result;
            var returnData = ReturnData.ConvertFromReturnData( result );

            if (returnData.errorCode == (int) ReturnData.ReturnCode.OK)
            {
                _sessionProvider.SetEmail(emailAddress);
            }

            return returnData;
        }

        public ReturnData ResetPasswordWithCode(string code, string verifyEmailAdress)
        {
            string emailAddress = _sessionProvider.GetEmail();
            _sessionProvider.RemoveSession();

            if ( verifyEmailAdress != emailAddress )
            {
                return new ReturnData(ReturnData.ReturnCode.ErrorInData, "Email address does not match the verified email.");
            }

            var result = Task.Run(async() => await _restClient.ResetPasswordWithCode(code, emailAddress)).Result;
            var returnData = ReturnData.ConvertFromReturnData( result );
            if ( returnData.errorCode == (int) ReturnData.ReturnCode.OK )
            {
                _sessionProvider.SetToken(returnData.message);
            }
            return returnData;
        }

        public ReturnData ResetPasswordWithToken(string password)
        {
            var token = _sessionProvider.GetToken();
            _sessionProvider.RemoveSession();

            var result = Task.Run(async() => await _restClient.ResetPasswordWithToken(token,password)).Result;
            return ReturnData.ConvertFromReturnData( result );
        }

        private AccountData ConvertFromRestData(WebAPIData apiData)
        {
            if ( apiData.errorCode == 0 )
            {
                var accountData = JsonConvert.DeserializeObject<AccountData>(apiData.message);
                return accountData;
            }
            else
            {
                var accountData = new AccountData();
                accountData.errorCode = (AccountData.ErrorCode)apiData.errorCode;
                accountData.errorMessage = apiData.message;
                return accountData;
            }
        }

        public static void SetupService( IServiceCollection services )
        {
            services.AddTransient<IAccountRestClient, AccountRestClient>();
            services.AddTransient<IAccountClient, AccountClient>();
            services.AddTransient<IAccountSessionProvider, AccountSessionProvider>();
        }
    }
}