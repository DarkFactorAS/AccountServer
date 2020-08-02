using System;
using System.Collections.Generic;

namespace AccountServer.Model
{
    public class CreateAccountData
    {
        public string nickname { get; set; }
        public string username { get; set; }
        public string password { get; set; }

        public CreateAccountData()
        {
        }
    }


}
