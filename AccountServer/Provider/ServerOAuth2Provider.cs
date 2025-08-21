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
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
        IList<OAuth2Client> _oauth2Clients;
        IServerOAuth2SessionProvider _sessionProvider;
        IServerOAuth2Repository _serverOAuth2Repository;
        private string _tokenIssuer = "serveroauth2";

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
            if (accountConfig != null && accountConfig.OAuth2Config != null)
            {
                var oauth2 = accountConfig.OAuth2Config;
                tokenExpiresInSeconds = oauth2.TokenExpiresInSeconds;
                _tokenIssuer = oauth2.ServerIssuer;
            }
        }

        private void GetOAuth2Clients()
       {
            if (_oauth2Clients == null)
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

            // TODO: Set minimum scope
            _sessionProvider.SetScope(client.Scope);

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

            //var sessionScope = _sessionProvider.GetScope();
            var jwtSecret = CreateString(32); // Generate a random secret for JWT signing

            string accessToken = GenerateJwtToken(jwtSecret, sessionClientId, _tokenIssuer, tokenExpiresInSeconds);

            var responseCode = new OAuth2CodeResponse
            {
                AccessToken = accessToken,
                State = sessionState,
                TokenType = "Bearer",
                ExpiresIn = tokenExpiresInSeconds
            };

            _sessionProvider.SetCode(null); // Clear code after use
            _sessionProvider.SetToken(accessToken);
            _sessionProvider.SetServerSecret(jwtSecret);
            _sessionProvider.SetIssuer(_tokenIssuer);

            return responseCode;
        }

        private OAuth2AuthResponse ReturnAuthError(int errorCode)
        {
            return ReturnWebAPIError(new OAuth2AuthResponse(), errorCode) as OAuth2AuthResponse;
        }

        private OAuth2CodeResponse ReturnOAuth2CodeError(int errorCode)
        {
            return ReturnWebAPIError(new OAuth2CodeResponse(), errorCode) as OAuth2CodeResponse;
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

        // TODO: Replace with DFCommonUtil.Crypt
        public string GenerateJwtToken(string secret, string audience, string issuer, uint expiresIn = 1)
        {
            // Enforce minimum secret length for security (e.g., 32 characters for HMAC-SHA256)
            if (string.IsNullOrEmpty(secret) || secret.Length < 32)
            {
                throw new ArgumentException("JWT secret is too short. It must be at least 32 characters long for adequate security.", nameof(secret));
            }
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var timeSpan = TimeSpan.FromSeconds(expiresIn);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                //Subject = new System.Security.Claims.ClaimsIdentity(claims.Select(c => new System.Security.Claims.Claim(c.Key, c.Value))),
                Expires = DateTime.UtcNow.Add(timeSpan),
                SigningCredentials = credentials,
                Audience = audience,
                Issuer = issuer
            };

            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private string CreateString(int stringLength)
        {
            Random rd = new Random();
            const string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";
            char[] chars = new char[stringLength];

            for (int i = 0; i < stringLength; i++)
            {
            const string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";
            char[] chars = new char[stringLength];

            for (int i = 0; i < stringLength; i++)
            {
                chars[i] = allowedChars[RandomNumberGenerator.GetInt32(0, allowedChars.Length)];
            }

            return new string(chars);
        }        
   }
}