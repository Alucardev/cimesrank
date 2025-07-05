using Microsoft.Data.SqlClient;
using System.Data;
using ApiRanking.Infrastructure.Data.Abstractions;

namespace ApiRanking.Infrastructure.Data;
public class ConnectionFactory : IConnectionFactory
{
    private readonly IConfiguration _configuration;

    public ConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IDbConnection CreateConnection(string connectionName)
    {
        var connectionString = _configuration.GetConnectionString(connectionName);
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException($"No se encontró una cadena de conexión con el nombre '{connectionName}'.");
        }

        return new SqlConnection(connectionString);
    }
}
