using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using DFCommonLib.Utils;
using DFCommonLib.Logger;
using DFCommonLib.Config;
using DFCommonLib.HttpApi;

using AccountCommon.SharedModel;
using AccountServer.Model;
using AccountServer.Repository;
using Newtonsoft.Json;
using System.Linq;

namespace AccountServer.Provider
{
    public interface IServerOAuth2Provider
    {
        string PingServer();
        OAuth2AuthResponse Auth(OAuth2ClientData clientData);
        OAuth2CodeResponse Code(OAuth2CodeData codeData);
        string VerifyToken(OAuth2VerifyTokenData tokenData);
    }

    public class ServerOAuth2Provider : IServerOAuth2Provider
    {
        IDFLogger<ServerOAuth2Provider> _logger;
        IList<OAuth2Client> _oauth2Clients;
        IServerOAuth2SessionProvider _sessionProvider;
        IServerOAuth2Repository _serverOAuth2Repository;

        const int INVALID_CLIENT_ID = 1001;
        const int INVALID_CREDENTIALS = 1002;

        private uint tokenExpiresInSeconds = 86400; // 24 hours

        public ServerOAuth2Provider(
            IServerOAuth2SessionProvider sessionProvider,
            IConfigurationHelper configurationHelper,
            IServerOAuth2Repository serverOAuth2Repository,
            IDFLogger<ServerOAuth2Provider> logger)
        {
            _logger = logger;
            _oauth2Clients = null;
            _sessionProvider = sessionProvider;
            _serverOAuth2Repository = serverOAuth2Repository;

            var accountConfig = configurationHelper.Settings as AccountConfig;
            if (accountConfig != null && accountConfig.OAuth2TokenExpiresInSeconds > 0)
            {
                tokenExpiresInSeconds = accountConfig.OAuth2TokenExpiresInSeconds;
            }
        }

        private void GetOAuth2Clients()
        {
            if ( _oauth2Clients == null )
            {
                _oauth2Clients = _serverOAuth2Repository.GetOAuth2Clients();
            }
        }

        public string PingServer()
        {
            _logger.LogInfo("PING-PONG");
            return "PONG";
        }

        public OAuth2AuthResponse Auth(OAuth2ClientData clientData)
        {
            GetOAuth2Clients();

            if (clientData == null)
            {
                return ReturnAuthError(INVALID_CLIENT_ID);
            }

            if (_oauth2Clients == null)
            {
                return ReturnAuthError(INVALID_CLIENT_ID);
            }

            if (_oauth2Clients.Count == 0)
            {
                return ReturnAuthError(INVALID_CLIENT_ID);
            }

            var client = _oauth2Clients.FirstOrDefault(c => c.ClientId == clientData.ClientId);
            if (client == null)
            {
                return ReturnAuthError(INVALID_CLIENT_ID);
            }

            if (client.ClientSecret != clientData.ClientSecret)
            {
                return ReturnAuthError(INVALID_CREDENTIALS);
            }

            string code = Guid.NewGuid().ToString();

            _sessionProvider.RemoveSession();
            _sessionProvider.SetClientId(client.ClientId);
            _sessionProvider.SetCode(code);
            _sessionProvider.SetState(clientData.State);

            return new OAuth2AuthResponse
            {
                Code = code,
                State = clientData.State,
            };
        }

        public OAuth2CodeResponse Code(OAuth2CodeData codeData)
        {
            var sessionClientId = _sessionProvider.GetClientId();
            if (string.IsNullOrEmpty(sessionClientId))
            {
                return ReturnOAuth2CodeError(INVALID_CREDENTIALS);
            }

            var sessionState = _sessionProvider.GetState();
            if (string.IsNullOrEmpty(sessionState))
            {
                return ReturnOAuth2CodeError(INVALID_CREDENTIALS);
            }

            if (codeData.State != sessionState)
            {
                return ReturnOAuth2CodeError(INVALID_CREDENTIALS);
            }

            var sessionCode = _sessionProvider.GetCode();
            if (string.IsNullOrEmpty(sessionCode))
            {
                return ReturnOAuth2CodeError(INVALID_CREDENTIALS);
            }

            if (sessionCode != codeData.Code)
            {
                return ReturnOAuth2CodeError(INVALID_CREDENTIALS);
            }

            string accessToken = Guid.NewGuid().ToString();

            var responseCode = new OAuth2CodeResponse
            {
                AccessToken = accessToken,
                State = sessionState,
                TokenType = "Bearer",
                ExpiresIn = tokenExpiresInSeconds
            };

            uint expiresWhen = (uint)DateTime.UtcNow.AddSeconds(tokenExpiresInSeconds).Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;

            _sessionProvider.RemoveSession();
            _sessionProvider.SetState(sessionState);
            _sessionProvider.SetToken(accessToken);
            _sessionProvider.SetExpiresWhen(expiresWhen);

            return responseCode;
        }

        public string VerifyToken(OAuth2VerifyTokenData tokenData)
        {
            if (tokenData == null || string.IsNullOrEmpty(tokenData.Token))
            {
                return "Invalid token data";
            }

            var token = tokenData.Token;

            // Here you would typically verify the token against a database or cache.
            // For this example, we will just log and return a success message.
            _logger.LogInfo($"Verifying token: {token}");

            var sessionToken = _sessionProvider.GetToken();
            if (sessionToken != token)
            {
                _sessionProvider.RemoveSession();
                return "Token is invalid";
            }

            uint timeNow = (uint)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            uint expiresWhen = _sessionProvider.GetExpiresWhen();
            if (expiresWhen < timeNow)
            {
                _sessionProvider.RemoveSession();
                return "Token has expired";
            }

            return "Token is valid";
        }

        private OAuth2AuthResponse ReturnAuthError(int errorCode)
        {
            return ReturnWebAPIError(new WebAPIData(), errorCode) as OAuth2AuthResponse;
        }

        private OAuth2CodeResponse ReturnOAuth2CodeError(int errorCode)
        {
            return ReturnWebAPIError(new WebAPIData(), errorCode) as OAuth2CodeResponse;
        }

        private WebAPIData ReturnWebAPIError(WebAPIData returnObject, int errorCode)
        {
            var sessionClientId = _sessionProvider.GetClientId();
            var message = GetErrorMessage(errorCode);
            _logger.LogError($"[{sessionClientId}] Error Code: {errorCode}, Message: {message}");

            returnObject.errorCode = errorCode;
            returnObject.message = message;

            return returnObject;
        }
        
        private string GetErrorMessage(int errorCode)
        {
            return errorCode switch
            {
                INVALID_CLIENT_ID => "Invalid client ID",
                INVALID_CREDENTIALS => "Invalid credentials",
                _ => "Unknown error"
            };
        }
   }
}