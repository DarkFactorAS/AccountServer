using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AccountServer.Model;
using AccountServer.Provider;

namespace AccountServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        ILogger<AccountController> _logger;
        IAccountProvider _provider;

        public AccountController(ILogger<AccountController> logger, IAccountProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }

        [HttpGet]
        [Route("PingServer")]
        public string PingServer()
        {
            return _provider.PingServer();
        }

        [HttpPut]
        [Route("LoginToken")]
        public AccountData LoginToken(LoginTokenData tokenData)
        {
            return _provider.LoginToken(tokenData);
        }

        [HttpPut]
        [Route("LoginAccount")]
        public AccountData LoginAccount(LoginData loginData)
        {
            return _provider.LoginAccount(loginData);
        }

        [HttpPut]
        [Route("CreateAccount")]
        public AccountData CreateAccount( CreateAccountData createAccountData )
        {
            return _provider.CreateAccount(createAccountData);
        }

        [HttpPut]
        [Route("ResetPasswordWithEmail")]
        public ReturnData ResetPasswordWithEmail( ResetPasswordDataEmail resetPasswordDataEmail )
        {
            return _provider.ResetPasswordWithEmail(resetPasswordDataEmail.emailAddress);
        }

        [HttpPut]
        [Route("ResetPasswordWithCode")]
        public ReturnData ResetPasswordWithCode( ResetPasswordDataCode resetPasswordDataCode )
        {
            return _provider.ResetPasswordWithCode(resetPasswordDataCode.code, resetPasswordDataCode.emailAddress);
        }

        [HttpPut]
        [Route("ResetPasswordWithToken")]
        public ReturnData ResetPasswordWithToken( ResetPasswordDataToken resetPasswordDataToken )
        {
            return _provider.ResetPasswordWithToken(resetPasswordDataToken.token, resetPasswordDataToken.password);
        }
    }
}
