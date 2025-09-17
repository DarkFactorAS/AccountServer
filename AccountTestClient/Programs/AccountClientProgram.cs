

using DFCommonLib.Config;
using DFCommonLib.Logger;
using AccountClientModule.Client;
using AccountTestClient;
using AccountCommon.SharedModel;
using DFCommonLib.Utils;

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
                _accountClient.SetEndpoint(config.AccountServer?.BaseUrl);
                // Assuming there's a method to set API key in the client
                // _accountClient.SetApiKey(config.ApiKey);

                var msg = string.Format("Connecting to API : {0}", config.AccountServer?.BaseUrl);
                DFLogger.LogOutput(DFLogLevel.INFO, "AccountTestClient", msg);
            }
        }

        public void Run()
        {
            // Example usage of the account client
            var pingResult = _accountClient.Ping().Result;
            DFLogger.LogOutput(DFLogLevel.INFO, "AccountClientProgram", $"Ping result: {pingResult}");

            // Additional operations can be added here, such as login or account creation
            // var loginData = new LoginData { ... };
            // var accountData = _accountClient.LoginAccount(loginData);
            // DFLogger.LogOutput(DFLogLevel.INFO, "AccountClientProgram", $"Logged in account: {JsonConvert.SerializeObject(accountData)}");

            var username = "testuser";
            var email = "test@example.com";
            var newCode = "1234";
            var oldPassword = DFCrypt.EncryptInput("OldPassword*123");
            var newPassword = DFCrypt.EncryptInput("TestPassword*123");

            var createResult = _accountClient.CreateAccount(new CreateAccountData
            {
                email = email,
                username = username,
                password = oldPassword,
                nickname = "TestUser"
            });
            DFLogger.LogOutput(DFLogLevel.WARNING, "AccountClientProgram", $"Create account result: {createResult.errorMessage}");

            var resetResultNoEmail = _accountClient.ResetPasswordWithCode(newCode, email);
            DFLogger.LogOutput(DFLogLevel.WARNING, "AccountClientProgram", $"Reset code (no email) result: {resetResultNoEmail.message}");

            var resetEmailResult = _accountClient.ResetPasswordWithEmail(email);
            DFLogger.LogOutput(DFLogLevel.WARNING, "AccountClientProgram", $"Reset email result: {resetEmailResult.message}");
            var resetCodeResult = _accountClient.ResetPasswordWithCode(newCode, email);
            DFLogger.LogOutput(DFLogLevel.WARNING, "AccountClientProgram", $"Reset code result: {resetCodeResult.message}");

            var token = resetCodeResult.message; // The token should come from the email reset operation
            var resetPasswordResult = _accountClient.ResetPasswordWithToken(token, newPassword);
            DFLogger.LogOutput(DFLogLevel.WARNING, "AccountClientProgram", $"Reset password result: {resetPasswordResult.message}");

            var loginResult = _accountClient.LoginAccount(new LoginData
            {
                username = username,
                password = newPassword
            });
            DFLogger.LogOutput(DFLogLevel.WARNING, "AccountClientProgram", $"Login account result: {loginResult.errorMessage}");
        }
    }
}