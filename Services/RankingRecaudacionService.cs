using System.Data;
using Dapper;
using ApiRanking.Core.Models;
using ApiRanking.Core.Abstractions;
using ApiRanking.Infrastructure.Data.Abstractions;
using ApiRanking.Core;

namespace ApiRanking.Services

{
    public class RankingRecaudacionService
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly IConnectionFactory _h2oDb;
        private readonly IConnectionFactory _aludevDb;
        private readonly ILogger<RankingRecaudacionService> _logger;

        public RankingRecaudacionService(IConnectionFactory h2db, IConnectionFactory aludevDb, ILogger<RankingRecaudacionService> logger)
        {
            _h2oDb = h2db;
            _aludevDb = aludevDb;
            _logger = logger;
        }

        public async Task<Result<RankingRecaudacionResponse>> GetRecaudacionNetaAsync()
        {
            var today = DateTime.Now;
            var currentMonth = new DateTime(today.Year, today.Month, 1);
            var previousMonth = currentMonth.AddMonths(-1);
            var twoMonthsAgo = currentMonth.AddMonths(-2);
            
            _logger.LogInformation("Verificando ranking para el mes: {CurrentMonth}", currentMonth.ToString("MMMM yyyy"));

            await _semaphore.WaitAsync();

            try
            {
                using var aludevDbConnection = _aludevDb.CreateConnection("AludevDb");
                
                // Verificar si ya existe un ranking para el mes actual
                var existingRanking = aludevDbConnection.QueryFirstOrDefault<(DateTime fecha_creacion, string ranking_data)>(
                    "SELECT TOP 1 fecha_creacion, ranking_data FROM rankings_recaudacion WHERE MONTH(fecha_creacion) = @Month AND YEAR(fecha_creacion) = @Year ORDER BY fecha_creacion DESC",
                    new { Month = currentMonth.Month, Year = currentMonth.Year }
                );

                if (existingRanking != default)
                {
                    _logger.LogInformation("Ya existe un ranking para el mes {CurrentMonth}. Retornando ranking existente.", currentMonth.ToString("MMMM yyyy"));
                    var data = System.Text.Json.JsonSerializer.Deserialize<List<RepartoRecaudacion>>(existingRanking.ranking_data);
                    return Result<RankingRecaudacionResponse>.Success(new RankingRecaudacionResponse { fecha_informe = existingRanking.fecha_creacion, Ranking = data });
                }

                _logger.LogInformation("Generando nuevo ranking para el mes anterior {PreviousMonth}.", previousMonth.ToString("MMMM yyyy"));
                _logger.LogInformation("Fechas del mes anterior: {PreviousMonthStart} - {PreviousMonthEnd}", 
                    previousMonth.ToString("dd/MM/yyyy"), currentMonth.AddDays(-1).ToString("dd/MM/yyyy"));
                using var h2oDbConnection = _h2oDb.CreateConnection("H2oDb");
                var resultList = new List<RepartoRecaudacion>();
                var previousMonthResults = new List<RepartoRecaudacion>();
                
                // Obtener datos del mes anterior (desde el primer día hasta el último día del mes anterior)
                var previousMonthStart = previousMonth;
                var previousMonthEnd = currentMonth.AddDays(-1);
                
                foreach (var id in RepartosList.Values)
                {
                    var results = h2oDbConnection.Query<RepartoRecaudacion>(
                        "WS_GET_RECAUDACION_NETA",
                        new
                        {
                            idreparto = id,
                            fecha_desde = previousMonthStart.ToString("dd/MM/yyyy"),
                            fecha_hasta = previousMonthEnd.ToString("dd/MM/yyyy")
                        },
                        commandType: CommandType.StoredProcedure).ToList();

                    foreach (var result in results)
                    {
                        result.reparto = id.ToString();
                        previousMonthResults.Add(result);
                    }
                }

                // Si no hay datos del mes anterior, retornar error
                if (previousMonthResults.Count == 0)
                {
                    _logger.LogWarning("No se encontraron datos de recaudación para el mes anterior {PreviousMonth}.", previousMonth.ToString("MMMM yyyy"));
                    return Result.Failure<RankingRecaudacionResponse>(new Error("Ranking.Empty", $"No se encontraron datos de recaudación para el mes anterior {previousMonth.ToString("MMMM yyyy")}."));
                }

                var sortedPreviousMonthResults = previousMonthResults.OrderByDescending(r => r.recaudacion_neta).ToList();

                var twoMonthsAgoResults = new List<RepartoRecaudacion>();
                // Obtener datos de dos meses atrás (desde el primer día hasta el último día de dos meses atrás)
                var twoMonthsAgoStart = twoMonthsAgo;
                var twoMonthsAgoEnd = previousMonth.AddDays(-1);
                
                _logger.LogInformation("Comparando con dos meses atrás: {TwoMonthsAgoStart} - {TwoMonthsAgoEnd}", 
                    twoMonthsAgoStart.ToString("dd/MM/yyyy"), twoMonthsAgoEnd.ToString("dd/MM/yyyy"));
                _logger.LogInformation("Fechas de comparación (dos meses atrás): {TwoMonthsAgoStart} - {TwoMonthsAgoEnd}", 
                    twoMonthsAgoStart.ToString("dd/MM/yyyy"), twoMonthsAgoEnd.ToString("dd/MM/yyyy"));

                foreach (var id in RepartosList.Values)
                {
                    var twoMonthsAgoData = h2oDbConnection.Query<RepartoRecaudacion>(
                        "WS_GET_RECAUDACION_NETA",
                        new
                        {
                            idreparto = id,
                            fecha_desde = twoMonthsAgoStart.ToString("dd/MM/yyyy"),
                            fecha_hasta = twoMonthsAgoEnd.ToString("dd/MM/yyyy")
                        },
                        commandType: CommandType.StoredProcedure).ToList();

                    foreach (var result in twoMonthsAgoData)
                    {
                        result.reparto = id.ToString();
                        twoMonthsAgoResults.Add(result);
                    }
                }

                var sortedTwoMonthsAgoResults = twoMonthsAgoResults.OrderByDescending(r => r.recaudacion_neta).ToList();

                foreach (var current in sortedPreviousMonthResults)
                {
                    var previous = sortedTwoMonthsAgoResults.FirstOrDefault(p => p.reparto == current.reparto);

                    if (previous != null)
                    {
                        var currentPosition = sortedPreviousMonthResults.IndexOf(current);
                        var previousPosition = sortedTwoMonthsAgoResults.IndexOf(previous);
                        current.estado = previousPosition - currentPosition;
                    }
                    else
                    {
                        current.estado = 0;
                    }
                }

                var rankingJson = System.Text.Json.JsonSerializer.Serialize(sortedPreviousMonthResults);
                aludevDbConnection.Execute(
                    "INSERT INTO rankings_recaudacion (ranking_data) VALUES (@RankingData)",
                    new { RankingData = rankingJson }
                );

                _logger.LogInformation("Ranking generado exitosamente para el mes anterior {PreviousMonth}.", previousMonth.ToString("MMMM yyyy"));
                return Result<RankingRecaudacionResponse>.Success(new RankingRecaudacionResponse { fecha_informe = DateTime.UtcNow, Ranking = sortedPreviousMonthResults });
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}