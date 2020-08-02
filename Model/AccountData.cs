using System;
using System.Collections.Generic;

namespace AccountServer.Model
{
    public class AccountData
    {
        public uint id { get; set; }
        public string nickname { get; set; }
        public string token { get; set; }

        public AccountData()
        {
        }
    }


}
