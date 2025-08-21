using DFCommonLib.DataAccess;
using DFCommonLib.Logger;

namespace AccountServer.Repository
{
    public class AccountDatabasePatcher : StartupDatabasePatcher
    {
        private static string PATCHER = "AccountServer";

        public AccountDatabasePatcher(IDBPatcher dbPatcher) : base(dbPatcher)
        {
        }

        public override bool RunPatcher()
        {
            base.RunPatcher();

            // User Accounts
            _dbPatcher.Patch(PATCHER,2, "CREATE TABLE `users` ("
            + " `id` int(11) NOT NULL AUTO_INCREMENT, " 
            + " `nickname` varchar(100) NOT NULL DEFAULT '', "
            + " `username` varchar(100) NOT NULL DEFAULT '', "
            + " `password` varchar(100) NOT NULL DEFAULT '', "
            + " `email` varchar(100) NOT NULL DEFAULT '', "
            + " `salt` tinyblob,"
            + " `flags` int(11) NOT NULL DEFAULT 0,"
            + " `created` datetime NOT NULL,"
            + " `updated` datetime NOT NULL,"
            + " PRIMARY KEY (`id`)"
            + ")"
            );

            // Tokens
            _dbPatcher.Patch(PATCHER,3, "CREATE TABLE `tokens` ("
            + " `userid` int(11) NOT NULL, " 
            + " `token` varchar(100) NOT NULL DEFAULT '', "
            + " `created` datetime NOT NULL,"
            + " PRIMARY KEY (`userid`,`token`)"
            + ")"
            );

            // Set AUTO_INCREMENT
            _dbPatcher.Patch(PATCHER,4, "ALTER TABLE users AUTO_INCREMENT=10001;");

            // Update users table to add lastlogin and numlogins
            _dbPatcher.Patch(PATCHER,5, "ALTER TABLE users ADD COLUMN `lastlogin` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP;");
            _dbPatcher.Patch(PATCHER,6, "ALTER TABLE users ADD COLUMN `numlogins` int(11) NOT NULL DEFAULT 0;");

            // Add logintype to users table
            _dbPatcher.Patch(PATCHER,7, "ALTER TABLE users ADD COLUMN `logintype` int(11) NOT NULL DEFAULT 0 AFTER flags;");

            // Add oauth2_clients table
            _dbPatcher.Patch(PATCHER, 8, "CREATE TABLE `oauth2_clients` ("
            + " `id` int(11) NOT NULL AUTO_INCREMENT, "
            + " `client_id` varchar(100) NOT NULL DEFAULT '', "
            + " `client_secret` varchar(100) NOT NULL DEFAULT '', "
            + " `scope` varchar(100) NOT NULL DEFAULT '', "
            + " PRIMARY KEY (`id`)"
            + ")"
            );

            return _dbPatcher.Successful();
        }
    }
}