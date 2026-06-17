using algotournament.Services;

namespace algotournament.Services
{
    public class DuelMatchTimerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DuelMatchTimerService> _logger;

        public DuelMatchTimerService(IServiceScopeFactory scopeFactory, ILogger<DuelMatchTimerService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var duelService = scope.ServiceProvider.GetRequiredService<DuelService>();
                    var matchService = scope.ServiceProvider.GetRequiredService<DuelMatchService>();

                    await duelService.ExpireStaleRoomsAsync();
                    await matchService.ExpireMatchesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in duel match timer service");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
