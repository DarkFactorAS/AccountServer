


using AccountServer.Provider;
using AccountCommon.SharedModel;
using Microsoft.AspNetCore.Mvc;

namespace AccountServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServerOAuth2Controller : ControllerBase
    {
        IServerOAuth2Provider _provider;

        public ServerOAuth2Controller(IServerOAuth2Provider provider)
        {
            _provider = provider;
        }

        [HttpGet]
        [Route("PingServer")]
        public string PingServer()
        {
            return _provider.PingServer();
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