using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Schedify.Data; // Adjust namespace according to your project
using Schedify.Models; // Assuming Event model is here
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Schedify.Services
{
    public class EventStatusUpdater : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventStatusUpdater> _logger;

        public EventStatusUpdater(IServiceProvider serviceProvider, ILogger<EventStatusUpdater> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EventStatusUpdater Service is running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        // Find events that need status updates
                        var now = DateTime.UtcNow;
                        var events = await dbContext.Events
                            .Where(e => (e.Status == EventStatus.Open) ||
                                        (e.Status == EventStatus.Ongoing))
                            .ToListAsync(stoppingToken);

                        foreach (var evt in events)
                        {
                        
                            if (evt.Status == EventStatus.Open && evt.StartAt <= now)
                            {
                                evt.Status = EventStatus.Ongoing;
                                _logger.LogInformation($"Event {evt.Id} started.");
                            }
                            else if (evt.Status == EventStatus.Ongoing && evt.EndAt <= now)
                            {
                                evt.Status = EventStatus.Completed; // Adjust according to your system
                                _logger.LogInformation($"Event {evt.Id} completed.");
                            }
                        }

                        if (events.Any())
                        {
                            await dbContext.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating event statuses.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Runs every minute
            }
        }
    }
}
