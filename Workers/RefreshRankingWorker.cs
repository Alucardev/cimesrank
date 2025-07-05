using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace ApiRanking.Services
{
    public class RefreshRankingWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RefreshRankingWorker> _logger;

        public RefreshRankingWorker(IServiceProvider serviceProvider, ILogger<RefreshRankingWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RefreshRankingWorker iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var RankingFiadosService = scope.ServiceProvider.GetRequiredService<RankingFiadoService>();
                        var RankingRecaudacionService = scope.ServiceProvider.GetRequiredService<RankingRecaudacionService>();

                        _logger.LogInformation("Refrescando data.");

                        // Llama a los métodos del servicio
                        await RankingRecaudacionService.GetRecaudacionNetaAsync();
                        await RankingFiadosService.GetFiadoRepartosAsync();

                        _logger.LogInformation("Proceso refrescado.");
                    }
                }                    
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ocurrio un error al refrescar la informacion.");
                }

                // Espera 24 horas antes de volver a ejecutar el proceso
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }

            _logger.LogInformation("Worker off");
        }
    }
}
 