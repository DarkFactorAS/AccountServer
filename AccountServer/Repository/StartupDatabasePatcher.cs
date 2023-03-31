using DFCommonLib.DataAccess;
using DFCommonLib.Logger;

namespace AccountServer.Repository
{
    public interface IStartupDatabasePatcher
    {
        bool RunPatcher();
    }

    public class StartupDatabasePatcher : IStartupDatabasePatcher
    {
        private static string PATCHER = "AccountServer";

        private IDBPatcher _dbPatcher;

        public StartupDatabasePatcher(IDBPatcher dbPatcher)
        {
            _dbPatcher = dbPatcher;
        }

        public bool RunPatcher()
        {
            _dbPatcher.Init();

            // Logtable
            _dbPatcher.Patch(PATCHER,1, "CREATE TABLE `logtable` ( `id` int(11) NOT NULL AUTO_INCREMENT, `created` datetime NOT NULL, `loglevel` int(11) NOT NULL, `groupname` varchar(100) NOT NULL DEFAULT '', `message` varchar(1024) NOT NULL DEFAULT '', PRIMARY KEY (`id`))");

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

            return _dbPatcher.Successful();
        }
    }
}