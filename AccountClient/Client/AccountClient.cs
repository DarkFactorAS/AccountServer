
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using DFCommonLib.HttpApi;
using Newtonsoft.Json;

using AccountCommon.SharedModel;
using AccountClientModule.RestClient;
using AccountClientModule.Provider;

namespace AccountClientModule.Client
{
    public interface IAccountClient
    {
        void SetEndpoint( string endpoint );
        string PingServer();
        AccountData LoginAccount(LoginData accountData);
        AccountData LoginToken(LoginTokenData accountData);
        AccountData LoginGameCenter(LoginData accountData);
        AccountData CreateAccount(CreateAccountData createAccountData);
        ReturnData ResetPasswordWithEmail(string emailAddress);
        ReturnData ResetPasswordWithCode(string code);
        ReturnData ResetPasswordWithToken(string password );
    }

    public class AccountClient : IAccountClient
    {
        IAccountRestClient _restClient;
        IAccountSessionProvider _sessionProvider;

        public AccountClient(IAccountRestClient restClient, IAccountSessionProvider sessionProvider)
        {
            _restClient = restClient;
            _sessionProvider = sessionProvider;
        }

        public void SetEndpoint( string endpoint )
        {
            _restClient.SetEndpoint(endpoint);
        }

        public string PingServer()
        {
            var result = Task.Run(async() => await _restClient.PingServer()).Result;
            return result.message;
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

        public AccountData LoginGameCenter(LoginData loginData)
        {
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
            var returnData = ConvertFromReturnData( result );

            if (returnData.code == ReturnData.ReturnCode.OK)
            {
                _sessionProvider.SetEmail(emailAddress);
            }

            return returnData;
        }

        public ReturnData ResetPasswordWithCode(string code)
        {
            string emailAddress = _sessionProvider.GetEmail();
            _sessionProvider.RemoveSession();

            var result = Task.Run(async() => await _restClient.ResetPasswordWithCode(code, emailAddress)).Result;
            var returnData = ConvertFromReturnData( result );
            if ( returnData.code == ReturnData.ReturnCode.OK )
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
            return ConvertFromReturnData( result );
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

        private ReturnData ConvertFromReturnData(WebAPIData apiData)
        {
            if ( apiData.errorCode == 0 )
            {
                var returnData = JsonConvert.DeserializeObject<ReturnData>(apiData.message);
                return returnData;
            }
            else
            {
                var returnData = new ReturnData();
                returnData.code = (ReturnData.ReturnCode) apiData.errorCode;
                returnData.message = apiData.message;
                return returnData;
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