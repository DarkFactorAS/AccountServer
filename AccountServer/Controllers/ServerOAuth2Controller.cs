


using AccountServer.Provider;
using AccountCommon.SharedModel;
using Microsoft.AspNetCore.Mvc;
using DFCommonLib.HttpApi;

namespace AccountServer.Controllers
{
    public class ServerOAuth2Controller : DFRestServerController
    {
        IServerOAuth2Provider _provider;

        public ServerOAuth2Controller(IServerOAuth2Provider provider)
        {
            _provider = provider;
        }

        [HttpPut]
        [Route("auth")]
        public OAuth2AuthResponse Auth(OAuth2ClientData clientData)
        {
            return _provider.Auth(clientData);
        }

        [HttpPut]
        [Route("code")]
        public OAuth2CodeResponse Code(OAuth2CodeData codeData)
        {
            return _provider.Code(codeData);
        }
    }
}