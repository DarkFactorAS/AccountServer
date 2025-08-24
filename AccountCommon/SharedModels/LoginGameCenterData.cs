using System;
using System.Collections.Generic;

namespace AccountCommon.SharedModel
{
    public class LoginGameCenterData
    {
        public string username { get; set; }
        public string password { get; set; }
        public string nickname { get; set; }

        public LoginGameCenterData()
        {
            username = string.Empty;
            password = string.Empty;
            nickname = string.Empty;
        }
    }
}
