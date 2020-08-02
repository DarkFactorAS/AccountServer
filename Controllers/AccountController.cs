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
        [Route("LoginAccount")]
        public AccountData LoginAccount(LoginData loginData)
        {
            return _repository.LoginAccount(loginData);
        }

        [HttpPut]
        [Route("CreateAccount")]
        public AccountData CreateAccount( CreateAccountData createAccountData )
        {
            return _repository.CreateAccount(createAccountData);
        }
    }
}
