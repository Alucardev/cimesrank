using System.Data;


namespace ApiRanking.Infrastructure.Data.Abstractions;
public interface IConnectionFactory
{
    IDbConnection CreateConnection(string connectionName);
}

