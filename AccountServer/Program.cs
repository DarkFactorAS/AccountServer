using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

using AccountServer.Repository;
using AccountServer.Provider;
using AccountServer.Model;

using DFCommonLib.Config;
using DFCommonLib.Logger;
using DFCommonLib.Utils;

using DarkFactor.MailClient;
using DFCommonLib.DataAccess;
using DFCommonLib.HttpApi.OAuth2;

namespace AccountServer
{
    class Program
    {
        public static string AppName = "AccountServer";
        public static string AppVersion = "1.2.7";

        static void Main(string[] args)
        {
            var builder = CreateHostBuilder(args).Build();

            try
            {
                IConfigurationHelper configurationHelper = DFServices.GetService<IConfigurationHelper>();
                var config = configurationHelper.Settings as AccountConfig;
                var msg = string.Format("Connecting to DB : {0}:{1}", config.DatabaseConnection.Server, config.DatabaseConnection.Port);
                DFLogger.LogOutput(DFLogLevel.INFO, "AccountServer", msg);

                // Run database script
                IStartupDatabasePatcher startupRepository = DFServices.GetService<IStartupDatabasePatcher>();
                startupRepository.WaitForConnection();
                if (startupRepository.RunPatcher())
                {
                    DFLogger.LogOutput(DFLogLevel.INFO, "Startup", "Database patcher ran successfully");
                }
                else
                {
                    DFLogger.LogOutput(DFLogLevel.ERROR, "Startup", "Database patcher failed");
                    Environment.Exit(1);
                    return;
                }

                builder.Run();
            }
            catch (Exception ex)
            {
                DFLogger.LogOutput(DFLogLevel.WARNING, "Startup", ex.ToString());
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddTransient<IConfigurationHelper, ConfigurationHelper<AccountConfig>>();

                new DFServices(services)
                    .SetupLogger()
                    .SetupMySql()
                    .LogToConsole(DFLogLevel.INFO)
                    .LogToMySQL(DFLogLevel.WARNING)
                    .LogToEvent(DFLogLevel.ERROR, AppName);
                ;


                services.AddTransient<IStartupDatabasePatcher, AccountDatabasePatcher>();
                services.AddTransient<IAccountSessionProvider, AccountSessionProvider>();
                services.AddTransient<IAccountRepository, AccountRepository>();
                services.AddTransient<IAccountProvider, AccountProvider>();
                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                OAuth2Server.SetupService(services);
                MailClient.SetupService(services);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            }
        );
    }
}
