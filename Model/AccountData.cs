using System;
using System.Collections.Generic;

namespace AccountServer.Model
{
    public class AccountData
    {
        public string nickname { get; set; }
        public string token { get; set; }

        public AccountData()
        {
        }

        public AccountData(string nickname, string token)
        {
            this.nickname = nickname;
            this.token = token;
        }
    }
}
