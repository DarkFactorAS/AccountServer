

using DFCommonLib.Config;
using DFCommonLib.Logger;
using AccountClientModule.Client;
using AccountTestClient;

namespace TestAccountClient
{
    public class AccountClientProgram
    {
        IAccountClient _accountClient;

        public AccountClientProgram(IAccountClient accountClient, IConfigurationHelper configurationHelper)
        {
            _accountClient = accountClient;

            // Set the API endpoint and key from the configuration
            var config = configurationHelper.Settings as TestAccountConfig;
            if (config != null)
            {
                _accountClient.SetEndpoint(config.AccountServer);
                // Assuming there's a method to set API key in the client
                // _accountClient.SetApiKey(config.ApiKey);

                var msg = string.Format("Connecting to API : {0}", config.AccountServer);
                DFLogger.LogOutput(DFLogLevel.INFO, "AccountTestClient", msg);
            }
        }

        public void Run()
        {
            // Example usage of the account client
            var pingResult = _accountClient.PingServer();
            DFLogger.LogOutput(DFLogLevel.INFO, "AccountClientProgram", $"Ping result: {pingResult}");

            // Additional operations can be added here, such as login or account creation
            // var loginData = new LoginData { ... };
            // var accountData = _accountClient.LoginAccount(loginData);
            // DFLogger.LogOutput(DFLogLevel.INFO, "AccountClientProgram", $"Logged in account: {JsonConvert.SerializeObject(accountData)}");
        }
    }
}