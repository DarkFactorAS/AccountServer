using System;
using System.Collections.Generic;

namespace AccountServer.Model
{
    public class CreateAccountData
    {
        public string username { get; set; }
        public string password { get; set; }
        public string nickname { get; set; }
        public string email { get; set; }

        public CreateAccountData()
        {
        }
    }
}
