using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SchedulerBot.Client.Scheduler
{
    public class QuartzHostedService : IHostedService
    {
        private readonly IEventScheduler _eventScheduler;
        private readonly ILogger<QuartzHostedService> _logger;

        public QuartzHostedService(IEventScheduler eventScheduler, ILogger<QuartzHostedService> logger)
        {
            _eventScheduler = eventScheduler;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Quartz event scheduler hosted service");

            await _eventScheduler.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Quartz event scheduler hosted service");

            await _eventScheduler.StopAsync(cancellationToken);
        }
    }
}
