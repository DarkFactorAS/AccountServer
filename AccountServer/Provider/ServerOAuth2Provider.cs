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
using Newtonsoft.Json;

namespace AccountServer.Provider
{
    public interface IServerOAuth2Provider
    {
        string PingServer();
        OAuth2AuthResponse Auth(OAuth2ClientData clientData);
        OAuth2CodeResponse Code(OAuth2CodeData codeData);
    }

    public class ServerOAuth2Provider : IServerOAuth2Provider
    {
        IDFLogger<ServerOAuth2Provider> _logger;
        OAuth2Clients _oauth2Clients;
        IServerOAuth2SessionProvider _sessionProvider;

        const int INVALID_CLIENT_ID = 1001;
        const int INVALID_CREDENTIALS = 1002;

        public ServerOAuth2Provider(IServerOAuth2SessionProvider sessionProvider,
            IDFLogger<ServerOAuth2Provider> logger)
        {
            _logger = logger;
            _oauth2Clients = new OAuth2Clients();
            _sessionProvider = sessionProvider;
        }

        public string PingServer()
        {
            _logger.LogInfo("PING-PONG");
            return "PONG";
        }

        public OAuth2AuthResponse Auth(OAuth2ClientData clientData)
        {
            if (clientData == null)
            {
                return ReturnAuthError(INVALID_CLIENT_ID);
            }

            var client = _oauth2Clients.GetClientById(clientData.ClientId);
            if (client == null)
            {
                return ReturnAuthError(INVALID_CLIENT_ID);
            }

            if (client.ClientSecret != clientData.ClientSecret)
            {
                return ReturnAuthError(INVALID_CREDENTIALS);
            }

            string code = Guid.NewGuid().ToString();

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
                ExpiresIn = 86400
            };

            return responseCode;
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