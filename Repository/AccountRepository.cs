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
        InternalAccountData CreateAccount(CreateAccountData createAccountData, byte[] salt);
        InternalAccountData GetAccount(string username);
        string SaveToken( uint userId, string token );
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

            return AccountData.Error(AccountData.ErrorCode.TokenLoginError);
        }

        public InternalAccountData CreateAccount(CreateAccountData createAccountData, byte[] salt)
        {
            var sql = @"insert into users (id,nickname,username,password,salt,flags,created,updated) 
                values(0, @nickname, @username, @password, @salt, 0, now(), now())";
            using (var cmd = _connection.CreateCommand(sql))
            {
                cmd.AddParameter("@nickname", createAccountData.nickname);
                cmd.AddParameter("@username", createAccountData.username);
                cmd.AddParameter("@password", createAccountData.password);
                cmd.AddClobParameter("@salt", salt);
                cmd.ExecuteNonQuery();
            }

            return GetAccount(createAccountData.username);
        }

        public InternalAccountData GetAccount(string username)
        {
            var sql = @"SELECT id, nickname, username, password, salt
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

                        // TODO - Change this
                        accountData.salt = new byte[16];
                        int index = reader.GetOrdinal("salt");
                        long numBytes = reader.GetBytes(index, 0, accountData.salt, 0, 16);

                        return accountData;
                    }
                }
            }
            return null;
        }

        public string SaveToken(uint userId, string token)
        {
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