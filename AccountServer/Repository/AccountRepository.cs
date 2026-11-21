using System;
using System.Collections.Generic;
using AccountCommon.SharedModel;
using AccountServer.Model;
using DFCommonLib.DataAccess;
using DFCommonLib.Logger;

namespace AccountServer.Repository
{
    public interface IAccountRepository
    {
        InternalAccountData CreateAccount(CreateAccountData createAccountData, byte[] salt, AccountLoginType logintype);
        InternalAccountData GetAccountWithUsername(string username);
        InternalAccountData GetAccountWithToken(string token);
        InternalAccountData GetAccountWithNickname(string nickname);
        InternalAccountData GetAccountWithEmail(string emailAdddress);
        InternalAccountData GetAccountWithId(uint accountId);
        void UpdateLastLogin(uint accountId);
        void PurgeOldTokens(uint userId, uint days);
        string SaveToken( uint userId, string token );
        void SetNewPassword( uint userId, string password );

        // Admin functions (AccountWeb)
        List<AdminUserData> SearchAccounts(string query, int limit);
        AdminUserData GetAdminAccount(uint accountId);
        void UpdateAccount(uint accountId, string nickname, string email);
        void SetPasswordAndSalt(uint accountId, string password, byte[] salt);
        void DeleteTokens(uint accountId);
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

        public InternalAccountData CreateAccount(CreateAccountData createAccountData, byte[] salt, AccountLoginType logintype)
        {
            var sql = @"insert into users (id,nickname,username,password,email,salt,flags,logintype,created,updated, lastlogin, numlogins) 
                values(0, @nickname, @username, @password, @email, @salt, 0, @logintype, now(), now(), now(), 1)";
            using (var cmd = _connection.CreateCommand(sql))
            {
                cmd.AddParameter("@nickname", createAccountData.nickname);
                cmd.AddParameter("@username", createAccountData.username);
                cmd.AddParameter("@password", createAccountData.password);
                cmd.AddParameter("@email", createAccountData.email);
                cmd.AddParameter("@logintype", (int)logintype);
                cmd.AddClobParameter("@salt", salt);
                cmd.ExecuteNonQuery();
            }

            return GetAccountWithUsername(createAccountData.username);
        }

        public InternalAccountData GetAccountWithNickname(string nickname)
        {
            return GetInternalAccount("nickname", nickname);
        }

        public InternalAccountData GetAccountWithToken(string token)
        {
            var accountId = GetAccountIdFromToken(token);
            return GetAccountWithId(accountId);
        }

        public InternalAccountData GetAccountWithId(uint accountId)
        {
            return GetInternalAccount("id", "" + accountId);
        }

        public InternalAccountData GetAccountWithUsername(string username)
        {
            return GetInternalAccount("username", username);
        }

        public InternalAccountData GetAccountWithEmail(string emailAdddress)
        {
            return GetInternalAccount("email", emailAdddress);
        }

        public InternalAccountData GetInternalAccount(string bindFieldname, string bindVariable)
        {
            string formattedSql = string.Format("SELECT id, nickname, username, password, salt, email, flags FROM users WHERE {0} = @bindVariable", bindFieldname);
            using (var cmd = _connection.CreateCommand(formattedSql))
            {
                cmd.AddParameter("@bindVariable", bindVariable);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        InternalAccountData accountData = new InternalAccountData();
                        accountData.id = Convert.ToUInt32(reader["id"]);
                        accountData.nickname = reader["nickname"].ToString();
                        accountData.username = reader["username"].ToString();
                        accountData.password = reader["password"].ToString();
                        accountData.email = reader["email"].ToString();
                        accountData.flags = Convert.ToUInt32(reader["flags"]);

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

        private uint GetAccountIdFromToken(string token)
        {
            string formattedSql = @"SELECT userid FROM tokens WHERE token = @bindVariable";
            using (var cmd = _connection.CreateCommand(formattedSql))
            {
                cmd.AddParameter("@bindVariable", token);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        InternalAccountData accountData = new InternalAccountData();
                        return Convert.ToUInt32(reader["userid"]);
                    }
                }
            }
            return 0;
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

        // Purge tokens older than 60 days for the given userId
        // This is called when a new token is created to ensure we don't have too many tokens stored.
        public void PurgeOldTokens(uint userId, uint days)
        {
            var sql = @"delete from tokens where userid = @userid and created < now() - interval @days day";
            using (var cmd = _connection.CreateCommand(sql))
            {
                cmd.AddParameter("@userid", userId);
                cmd.AddParameter("@days", days);
                cmd.ExecuteNonQuery();
            }
        }

        public void SetNewPassword(uint accountId, string password)
        {
            var sql = @"update users set password = @password, updated = now() where id = @accountId";
            using (var cmd = _connection.CreateCommand(sql))
            {
                cmd.AddParameter("@password", password);
                cmd.AddParameter("@accountId", accountId);
                cmd.ExecuteNonQuery();
            }
        }
        
        private const string AdminColumns = "id, username, nickname, email, flags, logintype, created, lastlogin, numlogins";

        public List<AdminUserData> SearchAccounts(string query, int limit)
        {
            var users = new List<AdminUserData>();
            var hasQuery = !string.IsNullOrWhiteSpace(query);
            // limit is an int chosen by the server, so it is safe to inline.
            var sql = hasQuery
                ? $"SELECT {AdminColumns} FROM users WHERE username LIKE @query OR nickname LIKE @query OR email LIKE @query ORDER BY id DESC LIMIT {limit}"
                : $"SELECT {AdminColumns} FROM users ORDER BY id DESC LIMIT {limit}";
            using (var cmd = _connection.CreateCommand(sql))
            {
                if (hasQuery)
                {
                    // Escape LIKE wildcards so the query is a plain substring match.
                    var escaped = query.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
                    cmd.AddParameter("@query", "%" + escaped + "%");
                }
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(ReadAdminAccount(reader));
                    }
                }
            }
            return users;
        }

        public AdminUserData GetAdminAccount(uint accountId)
        {
            var sql = $"SELECT {AdminColumns} FROM users WHERE id = @accountId";
            using (var cmd = _connection.CreateCommand(sql))
            {
                cmd.AddParameter("@accountId", accountId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return ReadAdminAccount(reader);
                    }
                }
            }
            return null;
        }

        private static AdminUserData ReadAdminAccount(System.Data.IDataRecord reader)
        {
            return new AdminUserData
            {
                id = Convert.ToUInt32(reader["id"]),
                username = reader["username"].ToString(),
                nickname = reader["nickname"].ToString(),
                email = reader["email"].ToString(),
                flags = Convert.ToUInt32(reader["flags"]),
                logintype = Convert.ToInt32(reader["logintype"]),
                created = Convert.ToDateTime(reader["created"]),
                lastlogin = Convert.ToDateTime(reader["lastlogin"]),
                numlogins = Convert.ToInt32(reader["numlogins"]),
            };
        }

        public void UpdateAccount(uint accountId, string nickname, string email)
        {
            var sql = @"update users set nickname = @nickname, email = @email, updated = now() where id = @accountId";
            using (var cmd = _connection.CreateCommand(sql))
            {
                cmd.AddParameter("@nickname", nickname);
                cmd.AddParameter("@email", email);
                cmd.AddParameter("@accountId", accountId);
                cmd.ExecuteNonQuery();
            }
        }

        public void SetPasswordAndSalt(uint accountId, string password, byte[] salt)
        {
            var sql = @"update users set password = @password, salt = @salt, updated = now() where id = @accountId";
            using (var cmd = _connection.CreateCommand(sql))
            {
                cmd.AddParameter("@password", password);
                cmd.AddClobParameter("@salt", salt);
                cmd.AddParameter("@accountId", accountId);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteTokens(uint accountId)
        {
            var sql = @"delete from tokens where userid = @accountId";
            using (var cmd = _connection.CreateCommand(sql))
            {
                cmd.AddParameter("@accountId", accountId);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateLastLogin(uint accountId)
        {
            var sql = @"update users set lastlogin = now(), numlogins = numlogins + 1 where id = @accountId";
            using (var cmd = _connection.CreateCommand(sql))
            {
                cmd.AddParameter("@accountId", accountId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}