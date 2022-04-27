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
            string audience = Environment.GetEnvironmentVariable("Audience");
            string issuer = Environment.GetEnvironmentVariable("Issuer");

            System.Console.WriteLine($"{secret}, {audience}, {issuer}");

            services.AddSingleton<IAuthManagerInfo, AuthManagerInfo>(s =>
                                                            new AuthManagerInfo()
                                                            {
                                                                Secret = secret!=null?secret:"DevelopmentSecret",
                                                                Audience = audience!=null?audience:"DevelopmentSecret",
                                                                Issuer = issuer!=null?issuer:"DevelopmentSecret"
                                                            }
                                                        );
            services.AddSingleton<IManager, Manager>();

        }
    }
}
