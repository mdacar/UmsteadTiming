using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Steeltoe.Extensions.Logging;

namespace ReaderEventAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDynamicConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureLogging((builderContext, loggingBuilder) =>
                    {
                        loggingBuilder.AddConsole();
                        loggingBuilder.AddDebug();
                        loggingBuilder.AddDynamicConsole();
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
