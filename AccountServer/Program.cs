using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using DFCommonLib.Config;
using DFCommonLib.Logger;
using DFCommonLib.Utils;

using AccountServer.Repository;

namespace AccountServer
{
    class Program
    {
        public static string AppName = "AccountServer";

        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddTransient<IConfigurationHelper, ConfigurationHelper>();

                new DFServices(services)
                    .SetupLogger()
                    .SetupMySql()
                    .LogToConsole(DFLogLevel.INFO)
                    .LogToMySQL(DFLogLevel.WARNING)
                    .LogToEvent(DFLogLevel.ERROR, AppName);
                ;

                services.AddTransient<IAccountRepository, AccountRepository>();
                services.AddTransient<IAccountProvider, AccountProvider>();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            }
        );
    }
}
