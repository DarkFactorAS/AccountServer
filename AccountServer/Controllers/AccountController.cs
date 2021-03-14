using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AccountServer.Model;
using AccountServer.Repository;

namespace AccountServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        ILogger<AccountController> _logger;
        IAccountRepository _repository;
        IAccountProvider _provider;

        public AccountController(ILogger<AccountController> logger, IAccountRepository repository, IAccountProvider provider)
        {
            _logger = logger;
            _repository = repository;
            _provider = provider;
        }

        [HttpPut]
        [Route("LoginToken")]
        public AccountData LoginToken([FromForm] string token)
        {
            return _repository.LoginToken(token);
        }

        [HttpPut]
        [Route("LoginAccount")]
        public AccountData LoginAccount(LoginData loginData)
        {
            return _provider.LoginAccount(loginData);
        }

        [HttpPut]
        [Route("Create")]
        public AccountData CreateAccount( CreateAccountData createAccountData )
        {
            return _provider.CreateAccount(createAccountData);
        }
    }
}
