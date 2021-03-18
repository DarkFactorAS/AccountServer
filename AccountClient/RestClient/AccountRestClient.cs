
using System.Threading.Tasks;
using DFCommonLib.HttpApi;
using AccountClientModule.Model;

namespace AccountClientModule.RestClient
{
    public interface IAccountRestClient
    {
        void SetEndpoint(string endpoint);
        Task<WebAPIData> LoginAccount(LoginData accountData);
        Task<WebAPIData> CreateAccount(CreateAccountData createAccountData);
    }

    public class AccountRestClient : DFRestClient, IAccountRestClient
    {
        private const int POST_CREATE = 1;
        private const int POST_LOGIN = 2;
        private string _endpoint;

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