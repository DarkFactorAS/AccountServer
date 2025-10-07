


using Microsoft.AspNetCore.Mvc;
using TestAccountClient;

namespace TestAccountClient
{

    public class AccountTestController : ControllerBase
    {
        private readonly IAccountClientProgram _client;

        public AccountTestController(IAccountClientProgram client)
        {
            _client = client;
        }

        [HttpGet]
        [Route("RunTests")]
        public void RunTests()
        {
            _client.Run();
        }
    }
}