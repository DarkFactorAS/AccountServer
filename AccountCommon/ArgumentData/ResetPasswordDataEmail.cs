using System;
using System.Collections.Generic;

namespace AccountCommon.SharedModel
{
    public class ResetPasswordDataEmail
    {
        public string emailAddress { get; set; }
        public ResetPasswordDataEmail()
        {
            emailAddress = string.Empty;
        }
    }
}
