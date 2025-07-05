namespace ApiRanking.Core.Models
{
    public class RepartoRecaudacion
    {
        public string reparto { get; set; }
        public decimal recaudacion_neta { get; set; }
        public int estado { get; set; } // Nuevo campo para el puesto
    }
}
