using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace VoiceModTest.Host
{
    public class Runner
    {
        private readonly ILogger<Runner> _logger;

        public Runner(ILogger<Runner> logger)
        {
            _logger = logger;
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("Starting runner...");
        }
    }
}
