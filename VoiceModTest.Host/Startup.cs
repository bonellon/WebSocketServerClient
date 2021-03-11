using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace VoiceModTest.Host
{
    public class Startup
    {
        public static IServiceProvider ConfigureServices(IServiceCollection services)
        {
            ConfigureLogging(services);

            //Add services

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
