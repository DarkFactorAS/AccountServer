using System;
using System.Collections.Generic;
using DFCommonLib.Config;

namespace AccountServer.Model
{
    public class MailServer
    {
        public string ServerAddress { get; set; }
        public string SenderName{ get; set;}
        public string SenderEmail{ get; set;}
        public string EmailSubject {get;set;}
        public string EmailBody {get;set;}
    }

    public class AccountCustomer : Customer
    {
        public MailServer mailServer{ get; set;}
    }
}
