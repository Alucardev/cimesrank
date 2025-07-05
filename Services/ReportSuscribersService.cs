using Dapper;
using ApiRanking.Infrastructure.Data.Abstractions;


namespace ApiRanking.Services
{
    public class SubscriberService
    {
        private readonly IConnectionFactory _aludevDb;

        public SubscriberService(IConnectionFactory aludevDb)
        {
            _aludevDb = aludevDb;
        }

        public async Task<List<string>> GetAllEmailsAsync()
        {
            using var connection = _aludevDb.CreateConnection("aludevDb");

            var emails = await connection.QueryAsync<string>(
                "SELECT email FROM system_report_subscribers"
            );

            return emails.AsList();
        }
    }
}
