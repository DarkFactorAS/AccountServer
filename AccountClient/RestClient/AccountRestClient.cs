
using System.Threading.Tasks;
using DFCommonLib.HttpApi;
using AccountClientModule.Model;

namespace AccountClientModule.RestClient
{
    public interface IAccountRestClient
    {
        Task<WebAPIData> LoginAccount(LoginData accountData);
        Task<WebAPIData> CreateAccount(CreateAccountData createAccountData);
    }

    public class AccountRestClient : DFRestClient, IAccountRestClient
    {
        private const int POST_CREATE = 1;
        private const int POST_LOGIN = 2;

        static string Endpoint = "http://localhost:5100";

        override protected string GetHostname()
        {
            return AccountRestClient.Endpoint;
        }

        override protected string GetModule()
        {
            return "Account";
        }

        public async Task<WebAPIData> LoginAccount(LoginData loginData)
        {
            var response = await PutJsonData(POST_LOGIN,"LoginAccount",loginData);
            return response;
        }

        public async Task<WebAPIData> CreateAccount(CreateAccountData createAccountData)
        {
            var response = await PutJsonData(POST_CREATE,"CreateAccount",createAccountData);
            return response;
        }
    }
}