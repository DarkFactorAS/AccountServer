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

            return _dbPatcher.Successful();
        }
    }
}