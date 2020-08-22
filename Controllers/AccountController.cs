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

        public AccountController(ILogger<AccountController> logger, IAccountRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        [HttpPut]
        [Route("LoginToken")]
        public AccountData LoginToken(string token)
        {
            return _repository.LoginToken(token);
        }

        [HttpPut]
        [Route("LoginAccount")]
        public AccountData LoginAccount(LoginData loginData)
        {
            return _repository.LoginAccount(loginData);
        }

        [HttpPut]
        [Route("Create")]
        public AccountData CreateAccount( CreateAccountData createAccountData )
        {
            return _repository.CreateAccount(createAccountData);
        }
    }
}
