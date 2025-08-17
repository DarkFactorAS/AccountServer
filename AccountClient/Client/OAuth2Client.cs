
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using DFCommonLib.HttpApi;
using Newtonsoft.Json;

using AccountCommon.SharedModel;
using AccountClientModule.RestClient;
using AccountClientModule.Provider;

namespace AccountClientModule.Client
{
    public interface IOAuth2Client : IDFClient
    {
        Task<OAuth2CodeResponse> LoginOAuth2Client(string clientId, string clientSecret, string scope);
    }

    public class OAuth2Client : DFClient, IOAuth2Client
    {
        IOAuth2RestClient _restClient;
        IAccountSessionProvider _sessionProvider;

        public OAuth2Client(IOAuth2RestClient restClient, IAccountSessionProvider sessionProvider) : base(restClient)
        {
            _restClient = restClient;
            _sessionProvider = sessionProvider;
        }

        public async Task<OAuth2CodeResponse> LoginOAuth2Client(string clientId, string clientSecret, string scope)
        {
            string state = "MEGASTATE"; // Guid.NewGuid().ToString();
            var oauth2ClientData = new OAuth2ClientData
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                RedirectUri = "redirect_uri",
                Scope = scope,
                State = state
            };

            OAuth2AuthResponse clientResult = await _restClient.LoginOAuth2Client(oauth2ClientData);
            if (clientResult.errorCode != 0)
            {
                return new OAuth2CodeResponse
                {
                    errorCode = clientResult.errorCode,
                    message = clientResult.message
                };
            }

            // Verify state
            if ( clientResult.State != state )
            {
                return new OAuth2CodeResponse
                {
                    errorCode = 1003, // Invalid state
                    message = "Invalid state returned from server."
                };
            }

            // Trade code for access token
            var oauth2CodeData = new OAuth2CodeData
            {
                Code = clientResult.Code,
                State = state
            };

            OAuth2CodeResponse codeResult = await _restClient.LoginOAuth2WithCode(oauth2CodeData);

            // Verify state
            if (codeResult.State != state)
            {
                return new OAuth2CodeResponse
                {
                    errorCode = 1003, // Invalid state
                    message = "Invalid state returned from server."
                };
            }

            return codeResult;
        }

        public static void SetupService(IServiceCollection services)
        {
            services.AddTransient<IOAuth2RestClient, OAuth2RestClient>();
            services.AddTransient<IOAuth2Client, OAuth2Client>();
        }
    }
}