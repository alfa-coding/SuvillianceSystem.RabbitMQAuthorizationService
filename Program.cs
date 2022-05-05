using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuvillianceSystem.RabbitMQ_Models.Concrete;
using SuvillianceSystem.RabbitMQ_Models.Infrastructure;
using SuvillianceSystem.RabbitMQAuthorizationService.Infrastructure;

namespace SuvillianceSystem.RabbitMQAuthorizationService
{
    class Program
    {
        static void Main(string[] args)
        {


            Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .ConfigureServices(services => services.AddSingleton<ServerMQ>())
                .Build()
                .Services
                .GetService<ServerMQ>()
                .ListenLoop();
        }
        private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            string secret = Environment.GetEnvironmentVariable("Secret");
            secret = String.IsNullOrEmpty(secret)?"DevelopmentSecret":secret;

            string audience = Environment.GetEnvironmentVariable("Audience");
            audience = String.IsNullOrEmpty(audience)?"DevelopmentSecret":audience;
            
            string issuer = Environment.GetEnvironmentVariable("Issuer");
            issuer = String.IsNullOrEmpty(issuer)?"DevelopmentSecret":issuer;

            System.Console.WriteLine($"{secret}, {audience}, {issuer}");

            services.AddSingleton<IAuthManagerInfo, AuthManagerInfo>(s =>
                                                            new AuthManagerInfo()
                                                            {
                                                                Secret = secret,
                                                                Audience = audience,
                                                                Issuer = issuer
                                                            }
                                                        );
            
            
            string queueHost = Environment.GetEnvironmentVariable("QueueHost");
            queueHost = String.IsNullOrEmpty(queueHost)?"localhost":queueHost;

            System.Console.WriteLine($"{queueHost}");

            services.AddSingleton<IConnectorFactoryInfo, ConnectorInfo>(s =>
                                                            new ConnectorInfo()
                                                            {
                                                                Host=queueHost
                                                            }
                                                        );
            services.AddSingleton<IManager, Manager>();

        }
    }
}
