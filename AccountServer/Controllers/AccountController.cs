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

        // ---- Admin functions (AccountWeb) ----------------------------------------------------
        // Only OAuth2 clients listed in AdminClients (default: AccountWeb) may call these. The
        // client is identified by the audience ("aud") of its access token.

        private bool IsAdminClient()
        {
            return _provider.IsAdminClient(User.FindAll("aud").Select(claim => claim.Value));
        }

        [HttpPut]
        [Route("AdminSearchUsers")]
        public AdminUserListData AdminSearchUsers(AdminSearchUsersData data)
        {
            if (!IsAdminClient())
            {
                return AdminUserListData.Error(AccountData.ErrorCode.NotAuthorized);
            }
            return _provider.AdminSearchUsers(data?.query);
        }

        [HttpPut]
        [Route("AdminGetUser")]
        public AdminUserData AdminGetUser(AdminGetUserData data)
        {
            if (!IsAdminClient())
            {
                return AdminUserData.Error(AccountData.ErrorCode.NotAuthorized);
            }
            return _provider.AdminGetUser(data?.id ?? 0);
        }

        [HttpPut]
        [Route("AdminUpdateUser")]
        public AdminUserData AdminUpdateUser(AdminUpdateUserData data)
        {
            if (!IsAdminClient())
            {
                return AdminUserData.Error(AccountData.ErrorCode.NotAuthorized);
            }
            return _provider.AdminUpdateUser(data);
        }

        [HttpPut]
        [Route("AdminSetPassword")]
        public AdminUserData AdminSetPassword(AdminSetPasswordData data)
        {
            if (!IsAdminClient())
            {
                return AdminUserData.Error(AccountData.ErrorCode.NotAuthorized);
            }
            return _provider.AdminSetPassword(data);
        }

        public override string Version()
        {
            return Program.AppVersion;
        }
    }
}
