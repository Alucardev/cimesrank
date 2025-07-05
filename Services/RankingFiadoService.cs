using System.Data;
using Dapper;
using ApiRanking.Core.Models;

using ApiRanking.Core.Abstractions;
using ApiRanking.Infrastructure.Data.Abstractions;
using ApiRanking.Services.Abstractions;
using ApiRanking.Core;

namespace ApiRanking.Services
{
  
    public class RankingFiadoService
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly IConnectionFactory _h2oDb;  
        private readonly IConnectionFactory _aludevDb;
        private readonly IReportService _reportService;
        private readonly SubscriberService _subscriberService;

        public RankingFiadoService(IConnectionFactory h2db,IConnectionFactory aludevDb, IReportService reportService, SubscriberService subscriberService)
        {
           _h2oDb = h2db;
           _aludevDb = aludevDb;
            _reportService = reportService;
            _subscriberService = subscriberService;
        }
        public async Task<Result<RankingFiadoResponse>> GetFiadoRepartosAsync()
        {
  
            string currentDate = DateService.GetYesterdayDate();
            await _semaphore.WaitAsync();

            try
            {
                using var aludevDbConnection = _aludevDb.CreateConnection("aludevDb");

                // Verificar si hay un ranking reciente en la base de datos
                var lastRanking = aludevDbConnection.QueryFirstOrDefault<(DateTime fecha_creacion, string ranking_data)>(
                    "SELECT TOP 1 fecha_creacion, ranking_data FROM rankings_fiado ORDER BY fecha_creacion DESC"
                );

                if (lastRanking != default && (DateTime.Now - lastRanking.fecha_creacion).TotalDays < 1)
                {
                    // Si el último ranking tiene menos de 1 día, devolverlo

                    var data = System.Text.Json.JsonSerializer.Deserialize<List<FiadoReparto>>(lastRanking.ranking_data);
                   return new RankingFiadoResponse { fecha_informe = lastRanking.fecha_creacion, Ranking = data };
                }

                using var h2oDbConnection = _h2oDb.CreateConnection("h2oDb");

                var resultList = new List<FiadoReparto>();

            
                foreach (var id in RepartosList.Values)
                {
                    var results = h2oDbConnection.Query<FiadoReparto>(
                        "WS_GET_FIADO_REPARTO",
                        new
                        {
                            idreparto = id,
                            fecha = currentDate,
                            incluye_ctacte = 0
                        },
                        commandType: CommandType.StoredProcedure).ToList();

                    foreach (var result in results)
                    {
                        result.reparto = id.ToString();
                        resultList.Add(result);
                    }
                }
                var sortedResultList = resultList.OrderByDescending(r => r.acobrar).ToList();

                // Si no hay datos, retornar error
                if (sortedResultList.Count == 0)
                {
                    return Result.Failure<RankingFiadoResponse>(new Error("RankingFiado.Empty", "No se encontraron datos de fiado para el período solicitado."));
                }

                var rankingJson = System.Text.Json.JsonSerializer.Serialize(sortedResultList);
                aludevDbConnection.Execute(
               "INSERT INTO rankings_fiado (ranking_data) VALUES (@RankingData)",
               new { RankingData = rankingJson }
                );


                var emails = await _subscriberService.GetAllEmailsAsync();
                await _reportService.SendFiadoReportAsync(emails, sortedResultList);
                return new RankingFiadoResponse { fecha_informe = DateTime.UtcNow, Ranking = sortedResultList };
            }
            finally
            {
                _semaphore.Release();
            }
        }
        public async Task<Result<RankingFiado>> GetStoredRankingAsync(DateOnly fecha)
        {
            {
                using var aludevDbConnection = _aludevDb.CreateConnection("aludevDb");

                var storedRanking = await aludevDbConnection.QueryFirstOrDefaultAsync<RankingFiado>(
                    "SELECT * FROM rankings_fiado WHERE fecha_creacion = @Fecha",
                    new { Fecha = fecha }
                );

                return storedRanking ?? new RankingFiado(); // Retorna un objeto vacío si no encuentra resultados
            }
        }
    }
}
