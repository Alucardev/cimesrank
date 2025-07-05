using System.Net;
using System.Net.Mail;

namespace ApiRanking.Infrastructure
{
    public class EmailService : IEmailService
    {
        private readonly SmtpClient _smtpClient;
        private readonly string _fromAddress;

        public EmailService(IConfiguration configuration)
        {
            var smtpSettings = configuration.GetSection("SmtpSettings");
            _fromAddress = smtpSettings["FromAddress"];
            var host = smtpSettings["Host"]; // "smtp.gmail.com"
            var port = int.Parse(smtpSettings["Port"]); // 587 (TLS) o 465 (SSL)
            var username = smtpSettings["Username"]; // "tucorreo@gmail.com" (debe ser completo)
            var password = smtpSettings["Password"]; // "TuContraseñaDeAplicación"
            var enableSsl = bool.Parse(smtpSettings["EnableSsl"]); // true

            _smtpClient = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network, // Importante para Gmail
                UseDefaultCredentials = false // Asegura que use tus credenciales personalizadas
            };
        }

        public async Task SendAsync(string recipient, string subject, string body)
        {
            var mailMessage = new MailMessage(_fromAddress, recipient, subject, body)
            {
                IsBodyHtml = true
            };

            await _smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendFiadoReportAsync(string to, string subject, string body)
        {
            await SendAsync(to, subject, body);
        }
    }
}