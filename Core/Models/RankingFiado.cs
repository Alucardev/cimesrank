namespace ApiRanking.Core.Models
{
    public class RankingFiado
    {
        public List<FiadoReparto> Data { get; set; } = new List<FiadoReparto>();
        public DateTime FechaCreacion { get; set; }
    }
}
