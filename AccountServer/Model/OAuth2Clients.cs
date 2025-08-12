

using System.Collections;
using System.Collections.Generic;
using AccountCommon.SharedModel;

namespace AccountServer.Model
{
    public class OAuth2Clients
    {
        public IList<OAuth2ClientData> _clients;

        public OAuth2Clients()
        {
            _clients = new List<OAuth2ClientData>();

            // HACK : TODO Populate the client list
            _clients.Add(new OAuth2ClientData
            {
                ClientId = "client_id",
                ClientSecret = "client_secret",
                RedirectUri = "https://localhost/callback",
                Scope = "read write",
                State = "state1"
            });
        }

        public OAuth2ClientData GetClientById(string clientId)
        {
            foreach (var client in _clients)
            {
                if (client.ClientId == clientId)
                {
                    return client;
                }
            }
            return null;
        }
    }
}