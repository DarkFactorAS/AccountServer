

using System.Collections;
using System.Collections.Generic;
using AccountCommon.SharedModel;

namespace AccountServer.Model
{
    public class OAuth2Client
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
    }
}