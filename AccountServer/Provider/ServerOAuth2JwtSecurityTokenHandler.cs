using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DFCommonLib.Utils;
using Microsoft.IdentityModel.Tokens;

namespace AccountServer.Provider
{
    public class ServerOAuth2JwtSecurityTokenHandler : JwtSecurityTokenHandler
    {
        IServerOAuth2SessionProvider _sessionProvider;

        public ServerOAuth2JwtSecurityTokenHandler()
        {
            _sessionProvider = DFServices.GetService<IServerOAuth2SessionProvider>();
        }

        public override ClaimsPrincipal ValidateToken(string token, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token cannot be null or empty.", nameof(token));
            }

            var serverSecret = _sessionProvider.GetServerSecret();
            if (string.IsNullOrEmpty(serverSecret))
            {
                throw new InvalidOperationException("Failed to retrieve server secret.");
            }

            var audience = _sessionProvider.GetClientId();
            if (string.IsNullOrEmpty(audience))
            {
                throw new InvalidOperationException("Failed to retrieve audience.");
            }

            var issuer = _sessionProvider.GetIssuer();
            if (string.IsNullOrEmpty(issuer))
            {
                throw new InvalidOperationException("Failed to retrieve issuer.");
            }

            validationParameters.ValidAudience = audience;
            validationParameters.ValidIssuer = issuer;
            validationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(serverSecret)); // Replace with your secret key

            return base.ValidateToken(token, validationParameters, out validatedToken);
        }
    }
}
