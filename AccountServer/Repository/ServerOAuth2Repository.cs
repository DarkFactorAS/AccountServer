using System;
using System.Collections.Generic;
using AccountCommon.SharedModel;
using AccountServer.Model;
using DFCommonLib.DataAccess;
using DFCommonLib.Logger;

namespace AccountServer.Repository
{
    public interface IServerOAuth2Repository
    {
        IList<OAuth2Client> GetOAuth2Clients();
    }

    public class ServerOAuth2Repository : IServerOAuth2Repository
    {
        private IDbConnectionFactory _connection;

        private readonly IDFLogger<ServerOAuth2Repository> _logger;

        public ServerOAuth2Repository(
            IDbConnectionFactory connection,
            IDFLogger<ServerOAuth2Repository> logger
            )
        {
            _connection = connection;
            _logger = logger;
        }

        public IList<OAuth2Client> GetOAuth2Clients()
        {
            var clients = new List<OAuth2Client>();
            string formattedSql = "SELECT id, client_id, client_secret, scope FROM oauth2_clients";
            using (var cmd = _connection.CreateCommand(formattedSql))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var client = new OAuth2Client
                        {
                            ClientId = reader["client_id"].ToString(),
                            ClientSecret = reader["client_secret"].ToString(),
                            Scope = reader["scope"].ToString()
                        };
                        clients.Add(client);
                    }
                }
            }
            return clients;
        }
    }
}