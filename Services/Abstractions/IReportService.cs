using ApiRanking.Core.Models;

namespace ApiRanking.Services.Abstractions
{ 
    public interface IReportService
    {
        Task SendReportAsync(IEnumerable<string> recipients, string subject, string body);
        Task SendFiadoReportAsync(IEnumerable<string> recipients, List<FiadoReparto> datos);
    }
}
