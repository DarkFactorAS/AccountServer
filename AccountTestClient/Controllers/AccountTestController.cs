


using Microsoft.AspNetCore.Mvc;
using TestAccountClient;

namespace TestAccountClient
{
    [ApiController]  
    [Route("[controller]")]
    public class AccountTestController : ControllerBase
    {
        private readonly IAccountClientProgram _client;

        public AccountTestController(IAccountClientProgram client) : base()
        {
            _client = client;
        }

        [HttpGet]
        [Route("RunTests")]
        public IActionResult RunTests()
        {
            _client.Run();
            return Ok();
        }
    }
}