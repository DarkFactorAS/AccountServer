
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using DFCommonLib.HttpApi;
using Newtonsoft.Json;

using AccountClientModule.Model;
using AccountClientModule.RestClient;

namespace AccountClientModule.Client
{
    public interface IAccountClient
    {
        void SetEndpoint( string endpoint );
        string PingServer();
        AccountData LoginAccount(LoginData accountData);
        AccountData LoginToken(LoginTokenData accountData);
        AccountData CreateAccount(CreateAccountData createAccountData);
        WebAPIData ResetPasswordWithEmail(string emailAddress);
        WebAPIData ResetPasswordWithCode(string code, string emailAdddress );
        WebAPIData ResetPasswordWithToken(string token, string password );
    }

    public class AccountClient : IAccountClient
    {
        IAccountRestClient _restClient;

        public AccountClient(IAccountRestClient restClient)
        {
            _restClient = restClient;
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

        public AccountData CreateAccount(CreateAccountData createAccountData)
        {
            var result = Task.Run(async() => await _restClient.CreateAccount(createAccountData)).Result;
            return ConvertFromRestData( result );
        }

        public WebAPIData ResetPasswordWithEmail(string emailAddress)
        {
            var result = Task.Run(async() => await _restClient.ResetPasswordWithEmail(emailAddress)).Result;
            return result;
        }

        public WebAPIData ResetPasswordWithCode(string code, string emailAddress)
        {
            var result = Task.Run(async() => await _restClient.ResetPasswordWithCode(code, emailAddress)).Result;
            return result;
        }

        public WebAPIData ResetPasswordWithToken(string token, string password)
        {
            var result = Task.Run(async() => await _restClient.ResetPasswordWithToken(token,password)).Result;
            return result;
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
        }
    }
}