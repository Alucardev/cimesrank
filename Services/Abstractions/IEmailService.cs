using System.Threading.Tasks;
public interface IEmailService
{
    Task SendAsync(string to, string subject, string body);
    Task SendFiadoReportAsync(string to, string subject, string body);
}

