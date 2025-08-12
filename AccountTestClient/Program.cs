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

using DFCommonLib.Config;
using DFCommonLib.Logger;
using DFCommonLib.Utils;
using DFCommonLib.DataAccess;
using AccountTestClient;
using AccountClientModule.Client;

namespace TestAccountClient
{
    public class Program
    {
        public static string AppName = "DFCommonLib.TestApp";

        public static void Main(string[] args)
        {
            var builder = CreateHostBuilder(args).Build();

            try
            {
                IConfigurationHelper configuration = DFServices.GetService<IConfigurationHelper>();
                IAccountClient accountClient = DFServices.GetService<IAccountClient>();

                var program = new AccountClientProgram(accountClient, configuration);
                program.Run();

                IOAuth2Client _restClient = DFServices.GetService<IOAuth2Client>();
                var p2 = new OAuth2ClientProgram(_restClient, configuration);
                p2.Run();   

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
                services.AddTransient<IConfigurationHelper, ConfigurationHelper<TestAccountConfig> >();

                new DFServices(services)
                    .SetupLogger()
//                    .SetupMySql()
                    .LogToConsole(DFLogLevel.INFO)
//                    .LogToMySQL(DFLogLevel.WARNING)
//                    .LogToEvent(DFLogLevel.ERROR, AppName);
                ;
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            }
        );
    }
}
