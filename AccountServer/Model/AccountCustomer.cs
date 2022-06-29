using System;
using System.Collections.Generic;
using DFCommonLib.Config;

namespace AccountServer.Model
{
    public class AccountCustomer : Customer
    {
        public string MailServer { get; set; }
    }
}
