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

namespace AccountServer
{
    class Program
    {
        public static string AppName = "AccountServer";
        public static string AppVersion = "1.0.0";

        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                DFServices.Create(services);

                services.AddTransient<IConfigurationHelper, ConfigurationHelper<AccountCustomer> >();

                new DFServices(services)
                    .SetupLogger()
                    .SetupMySql()
                    .LogToConsole(DFLogLevel.INFO)
                    .LogToMySQL(DFLogLevel.WARNING)
                    .LogToEvent(DFLogLevel.ERROR, AppName);
                ;

                services.AddTransient<IStartupDatabasePatcher, StartupDatabasePatcher>();
                services.AddTransient<IAccountSessionProvider, AccountSessionProvider>();
                services.AddTransient<IAccountRepository, AccountRepository>();
                services.AddTransient<IAccountProvider, AccountProvider>();
                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                MailClient.SetupService(services);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            }
        );
    }
}
