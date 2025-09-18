using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AccountCommon.SharedModel;
using AccountServer.Model;
using AccountServer.Provider;
using DFCommonLib.HttpApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using DFCommonLib.HttpApi.OAuth2;

namespace AccountServer.Controllers
{
    // TODO: Renable this when client is ready
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AccountController : DFRestOAuth2ServerController
    {
        IAccountProvider _provider;

        public AccountController(IAccountProvider provider)
        {
            _provider = provider;
        }

        [HttpPut]
        [Route("LoginAuthFunction")]
        public AccountData LoginAuthFunction(LoginData loginData)
        {
            return _provider.LoginAccount(loginData);
        }

        [HttpPut]
        [Route("LoginAccount")]
        public AccountData LoginAccount(LoginData loginData)
        {
            return _provider.LoginAccount(loginData);
        }

        [HttpPut]
        [Route("LoginToken")]
        public AccountData LoginToken(LoginTokenData tokenData)
        {
            return _provider.LoginToken(tokenData);
        }

        [HttpPut]
        [Route("LoginGameCenter")]
        public AccountData LoginGameCenter(LoginGameCenterData loginData)
        {
            return _provider.LoginGameCenter(loginData);
        }

        [HttpPut]
        [Route("CreateAccount")]
        public AccountData CreateAccount(CreateAccountData createAccountData)
        {
            return _provider.CreateAccount(createAccountData);
        }

        [HttpPut]
        [Route("ResetPasswordWithEmail")]
        public ReturnData ResetPasswordWithEmail(ResetPasswordDataEmail resetPasswordDataEmail)
        {
            return _provider.ResetPasswordWithEmail(resetPasswordDataEmail.emailAddress);
        }

        [HttpPut]
        [Route("ResetPasswordWithCode")]
        public ReturnData ResetPasswordWithCode(ResetPasswordDataCode resetPasswordDataCode)
        {
            return _provider.ResetPasswordWithCode(resetPasswordDataCode.code, resetPasswordDataCode.emailAddress);
        }

        [HttpPut]
        [Route("ResetPasswordWithToken")]
        public ReturnData ResetPasswordWithToken(ResetPasswordDataToken resetPasswordDataToken)
        {
            return _provider.ResetPasswordWithToken(resetPasswordDataToken.token, resetPasswordDataToken.password);
        }
    }
}
