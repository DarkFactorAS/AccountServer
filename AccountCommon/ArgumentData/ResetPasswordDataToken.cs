using System;
using System.Collections.Generic;

namespace AccountCommon.SharedModel
{
    public class ResetPasswordDataToken
    {
        public string token { get; set; }
        public string password { get; set; }
        public ResetPasswordDataToken()
        {
        }
    }
}
