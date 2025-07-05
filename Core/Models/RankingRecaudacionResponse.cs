namespace ApiRanking.Core.Models
{
    public class RankingRecaudacionResponse
    {
        public DateTime fecha_informe { get; set; }
        public List<RepartoRecaudacion> Ranking { get; set; }
    }
}
