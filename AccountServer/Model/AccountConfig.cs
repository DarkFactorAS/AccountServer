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

    public class OAuth2Config
    {
        public uint TokenExpiresInSeconds { get; set; } = 86400;
        public string ServerIssuer { get; set; } = "serveroauth2";
    }

    public class AccountConfig : AppSettings
    {
        public MailServer mailServer { get; set; }
        public OAuth2Config OAuth2Config { get; set; } = new OAuth2Config();
        public uint LoginTokenHistoryExpirationDays { get; set; } = 180;
    }
}
