using System;
using System.Collections.Generic;
using AccountServer.Model;
using DFCommonLib.DataAccess;
using DFCommonLib.Logger;

namespace AccountServer.Repository
{
    public interface IAccountRepository
    {
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

        public AccountData LoginAccount(LoginData loginData)
        {
            AccountData accountData = GetAccount(loginData.username);
            if ( accountData != null )
            {
                if ( accountData.token == loginData.password )
                {
                    accountData.token = CreateToken(accountData.id);
                    return accountData;
                }
            }
            return null;
        }

        public AccountData CreateAccount(CreateAccountData createAccountData)
        {
            AccountData accountData = GetAccount(createAccountData.username);
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
                accountData.token = CreateToken(accountData.id);
                return accountData;
            }

            return null;
        }

        private AccountData GetAccount(string username)
        {
            // Get specific playfield
            var sql = @"SELECT id,nickname, password 
                FROM users 
                WHERE username = @username";
            using (var cmd = _connection.CreateCommand(sql))
            {
                cmd.AddParameter("@username", username);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        AccountData accountData = new AccountData();
                        accountData.id = Convert.ToUInt32(reader["id"]);
                        accountData.nickname = reader["nickname"].ToString();
                        accountData.token = reader["password"].ToString(); 
                        return accountData;
                    }
                }
            }
            return null;
        }

        private string CreateToken(uint userId)
        {
            string token = "123";

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