using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;


namespace TimeEntryFactory
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                })
                .ConfigureAppConfiguration((context, config) =>
                {
                    var builtConfig = config.Build();

                    var keyVaultUrl = builtConfig["KeyVaultUrl"];
                    var tenantId = builtConfig["KeyVaultTenantId"];
                    var clientId = builtConfig["KeyVaultClientId"];
                    var clientSecret = builtConfig["KeyVaultClientSecretId"];

                    var creds = new ClientSecretCredential(tenantId, clientId, clientSecret);
                    var client = new SecretClient(new Uri(keyVaultUrl), creds);
                    config.AddAzureKeyVault(client, new AzureKeyVaultConfigurationOptions());
                });
    }
}
