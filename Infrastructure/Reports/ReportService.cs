using ApiRanking.Core.Models;
using ApiRanking.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiRanking.Infrastructure
{
    internal sealed class ReportService : IReportService
    {
        private readonly EmailService _emailService;

        public ReportService(EmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task SendReportAsync(IEnumerable<string> recipients, string subject, string body)
        {
            foreach (var recipient in recipients)
            {
                await _emailService.SendAsync(recipient, subject, body);
            }
        }

        public async Task SendFiadoReportAsync(IEnumerable<string> recipients, List<FiadoReparto> datos)
        {
            string subject = "Reporte de Fiados";
            string body = HtmlBuilder.GenerarTablaFiadoHtml(datos);

            await SendReportAsync(recipients, subject, body);
        }
    }

    internal static class HtmlBuilder
    {
        public static string GenerarTablaFiadoHtml(List<FiadoReparto> datos)
        {
            StringBuilder html = new StringBuilder();

            html.AppendLine("<html>");
            html.AppendLine("<body style='font-family: Arial, sans-serif;'>");

            // Encabezado bonito
            html.AppendLine("<h2 style='color: #2E86C1;'>Reporte diario</h2>");
            html.AppendLine("<p>Detalle del informe:</p>");
            html.AppendLine($"<p>{DateTime.Now:dd/MM/yyyy}</p>");


            // Inicio de tabla
            html.AppendLine("<table style='border-collapse: collapse; width: 100%;'>");
            html.AppendLine("<thead>");
            html.AppendLine("<tr>");
            html.AppendLine("<th style='border: 1px solid #dddddd; padding: 8px; background-color: #f2f2f2;'>Puesto</th>");
            html.AppendLine("<th style='border: 1px solid #dddddd; padding: 8px; background-color: #f2f2f2;'>Reparto</th>");
            html.AppendLine("<th style='border: 1px solid #dddddd; padding: 8px; background-color: #f2f2f2;'>Exceso</th>");
            html.AppendLine("</tr>");
            html.AppendLine("</thead>");
            html.AppendLine("<tbody>");

            // Cuerpo de la tabla
            var index = 0;
            foreach (var item in datos)
            {
                html.AppendLine("<tr>");
                html.AppendLine($"<td style='border: 1px solid #dddddd; padding: 8px; text-align: center;'>{index+1}</td>");
                html.AppendLine($"<td style='border: 1px solid #dddddd; padding: 8px; text-align: center;'>{item.reparto}</td>");
                html.AppendLine($"<td style='border: 1px solid #dddddd; padding: 8px; text-align: right;'>{item.acobrar:N2}</td>");
                html.AppendLine("</tr>");
                index++;
            }

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");

            // Footer del correo
            html.AppendLine("<p style='margin-top: 20px; font-size: 12px; color: #888;'>Este es un correo automático. No responda a este mensaje.</p>");

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }
    }
}
