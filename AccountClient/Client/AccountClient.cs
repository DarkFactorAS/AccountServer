
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
        AccountData LoginAccount(LoginData accountData);
        AccountData CreateAccount(CreateAccountData createAccountData);
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

        public AccountData LoginAccount(LoginData loginData)
        {
            var result = Task.Run(async() => await _restClient.LoginAccount(loginData)).Result;
            return ConvertFromRestData( result );
        }

        public AccountData CreateAccount(CreateAccountData createAccountData)
        {
            var result = Task.Run(async() => await _restClient.CreateAccount(createAccountData)).Result;
            return ConvertFromRestData( result );
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