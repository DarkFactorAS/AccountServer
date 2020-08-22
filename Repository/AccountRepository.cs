using System;
using System.Collections.Generic;
using AccountServer.Model;
using DFCommonLib.DataAccess;
using DFCommonLib.Logger;

namespace AccountServer.Repository
{
    public interface IAccountRepository
    {
        AccountData LoginToken(string token);
        AccountData LoginAccount(LoginData accountData);
        AccountData CreateAccount(CreateAccountData createAccountData);
    }

    public class AccountRepository : IAccountRepository
    {
        private IDbConnectionFactory _connection;

        private readonly IDFLogger<AccountRepository> _logger;

        public AccountRepository(
            IDbConnectionFactory connection,
            IDFLogger<AccountRepository> logger
            )
        {
            _connection = connection;
            _logger = logger;
        }

        public AccountData LoginToken(string token)
        {
            // Get specific playfield
            var sql = @"SELECT u.id, u.nickname, t.token 
                FROM users u, tokens t 
                WHERE u.id = t.userid
                AND t.token = @token";
            using (var cmd = _connection.CreateCommand(sql))
            {
                cmd.AddParameter("@token", token);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        AccountData accountData = new AccountData();
                        accountData.nickname = reader["nickname"].ToString();
                        accountData.token = reader["token"].ToString(); 
                        return accountData;
                    }
                }
            }
            return null;
        }

        public AccountData LoginAccount(LoginData loginData)
        {
            InternalAccountData accountData = GetAccount(loginData.username);
            if ( accountData != null && accountData.password == loginData.password )
            {
                string token = CreateToken(accountData.id);
                return new AccountData( accountData.nickname, token );
            }
            return null;
        }

        public AccountData CreateAccount(CreateAccountData createAccountData)
        {
            InternalAccountData accountData = GetAccount(createAccountData.username);
            if ( accountData != null )
            {
                return null;
            }

            var sql = @"insert into users (id,nickname,username,password,created,updated) 
                values(0, @nickname, @username, @password, now(), now())";
            using (var cmd = _connection.CreateCommand(sql))
            {
                cmd.AddParameter("@nickname", createAccountData.nickname);
                cmd.AddParameter("@username", createAccountData.username);
                cmd.AddParameter("@password", createAccountData.password);
                cmd.ExecuteNonQuery();
            }

            accountData = GetAccount(createAccountData.username);
            if ( accountData != null )
            {
                string token = CreateToken(accountData.id);
                return new AccountData( accountData.nickname, token );
            }

            return null;
        }

        private InternalAccountData GetAccount(string username)
        {
            var sql = @"SELECT id, nickname, username, password
                FROM users 
                WHERE username = @username";
            using (var cmd = _connection.CreateCommand(sql))
            {
                cmd.AddParameter("@username", username);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        InternalAccountData accountData = new InternalAccountData();
                        accountData.id = Convert.ToUInt32(reader["id"]);
                        accountData.nickname = reader["nickname"].ToString();
                        accountData.username = reader["username"].ToString();
                        accountData.password = reader["password"].ToString();
                        return accountData;
                    }
                }
            }
            return null;
        }

        private string CreateToken(uint userId)
        {
            string token = Guid.NewGuid().ToString();

            var sql = @"insert into tokens (userid,token,created) 
                values(@userid,@token, now())";
            using (var cmd = _connection.CreateCommand(sql))
            {
                cmd.AddParameter("@userid", userId);
                cmd.AddParameter("@token", token);
                cmd.ExecuteNonQuery();
            }
            return token;
        }

    }
}