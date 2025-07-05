namespace ApiRanking.Core.Models
{
    public class RankingFiadoResponse
    {
        public DateTime fecha_informe { get; set; }
        public List<FiadoReparto> Ranking { get; set; }
    }
}
