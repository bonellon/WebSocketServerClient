using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace VoiceModTest.Host
{
    /// <summary>
    /// The application entry point
    /// </summary>
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // Check if port was correctly supplied.
            if (args.Length != 1)
            {
                Console.WriteLine("Error. Missing port argument");
                Console.WriteLine("Usage: <executableName> <portNumber>");

                return 1;
            }


            if (!ushort.TryParse(args[0], out ushort port))
            {
                Console.WriteLine("Port not in correct format. Please make sure port is UInt16 value");
                return 2;
            }

            var services = new ServiceCollection();
            var serviceProvider = Startup.ConfigureServices(services);

            using var applicationScope = serviceProvider.CreateScope();

            var runner = applicationScope.ServiceProvider.GetService<Runner>();

            await runner.RunAsync(port);
            return 0;
        }

    }
}
