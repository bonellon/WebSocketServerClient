using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VoiceModTest.Contracts;
using VoiceModTest.Host.services;

namespace VoiceModTest.Host
{
    /// <summary>
    /// Startup class responsible for Dependency Injection
    /// </summary>
    public class Startup
    {
        public static IServiceProvider ConfigureServices(IServiceCollection services)
        {
            ConfigureLogging(services);

            //Add services
            services.AddScoped<Runner>();
            services.AddScoped<IMessageServer, MessageServer>();
            services.AddScoped<IMessageClient, MessageClient>();

            return services.BuildServiceProvider();
        }

        private static void ConfigureLogging(IServiceCollection services)
        {
            var logger = new LoggerConfiguration()
                .WriteTo.File("..\\..\\..\\logs\\log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => Log.CloseAndFlush();

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddSerilog(logger, true);
            });
        }
    }
}
