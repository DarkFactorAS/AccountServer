

using DFCommonLib.Config;
using DFCommonLib.Logger;
using AccountClientModule.Client;
using AccountTestClient;
using DFCommonLib.HttpApi;
using AccountCommon.SharedModel;
using AccountClientModule.RestClient;

namespace TestAccountClient
{
    public class OAuth2ClientProgram
    {
        string programName = "OAuth2ClientProgram";

        IOAuth2Client _oAuth2Client;
        
        AccountServer _accountServerConfig;

        public OAuth2ClientProgram(IOAuth2Client oAuth2Client, IConfigurationHelper configurationHelper)
        {
            _oAuth2Client = oAuth2Client;

            // Set the API endpoint and key from the configuration
            var config = configurationHelper.Settings as TestAccountConfig;
            if (config != null)
            {
                _accountServerConfig = config.AccountServer;
                if (_accountServerConfig == null)
                {
                    throw new ArgumentNullException(nameof(config.AccountServer), "Account server configuration is missing.");
                }

                _oAuth2Client.SetEndpoint(_accountServerConfig.BaseUrl);
                // Assuming there's a method to set API key in the client
                // _oAuth2Client.SetApiKey(config.ApiKey);

                var msg = string.Format("Connecting to API : {0}", _accountServerConfig.BaseUrl);
                DFLogger.LogOutput(DFLogLevel.INFO, programName, msg);
            }
        }

        // private async Task EnsureAuthenticated()
        // {
        //     if (_oAuthSettings != null && (string.IsNullOrEmpty(_accessToken) || DateTime.UtcNow >= _tokenExpiry))
        //     {
        //         var requestBody = new Dictionary<string, string>
        //         {
        //             { "client_id", _oAuthSettings.ClientId },
        //             { "client_secret", _oAuthSettings.ClientSecret },
        //             { "grant_type", "client_credentials" }
        //         };

        //         var requestContent = new FormUrlEncodedContent(requestBody);
        //         var fullUrl = GetFullUrl("Auth");
        //         var response = await client.PostAsync(fullUrl, requestContent);

        //         if (!response.IsSuccessStatusCode)
        //         {
        //             throw new Exception("Failed to authenticate: " + response.ReasonPhrase);
        //         }

        //         var responseContent = await response.Content.ReadAsStringAsync();
        //         var tokenData = JsonConvert.DeserializeObject<OAuthTokenResponse>(responseContent);

        //         _accessToken = tokenData.AccessToken;
        //         _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenData.ExpiresIn);
        //     }
        // }

        public void Run()
        {
            // Example usage of the account client
            var pingResult = _oAuth2Client.PingServer();
            DFLogger.LogOutput(DFLogLevel.INFO, programName, $"Ping result: {pingResult}");

            OAuth2CodeResponse codeResponse = _oAuth2Client.LoginOAuth2Client(_accountServerConfig.ClientId, _accountServerConfig.ClientSecret, "read").Result;

            DFLogger.LogOutput(DFLogLevel.INFO, programName, $"Login response: {codeResponse.errorCode} : {codeResponse.message}");
            if (codeResponse is OAuth2CodeResponse)
            {
                DFLogger.LogOutput(DFLogLevel.INFO, programName, $"Login Code response: {codeResponse.AccessToken} : {codeResponse.TokenType}");
            }

            // Additional operations can be added here, such as login or account creation
            // var loginData = new LoginData { ... };
            // var accountData = _accountClient.LoginAccount(loginData);
            // DFLogger.LogOutput(DFLogLevel.INFO, programName, $"Logged in account: {JsonConvert.SerializeObject(accountData)}");
        }
    }
}