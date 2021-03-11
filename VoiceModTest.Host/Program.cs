using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace VoiceModTest.Host
{
    /// <summary>
    /// The application entry point
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            var serviceProvider = Startup.ConfigureServices(services);

            using var applicationScope = serviceProvider.CreateScope();

            var runner = applicationScope.ServiceProvider.GetService<Runner>();

            await runner.RunAsync();
        }

    }
}
