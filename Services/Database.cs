using System.Data;
using Npgsql;

namespace TeltonikaDataServer.Services;

public class Database
{
    private readonly string _connectionString;
    
    public Database(IConfiguration config)
    {
        _connectionString = config.GetValue<string>("DbConnString");
    }

    public IDbConnection GetConnection(string connectionString)
    {
        return new NpgsqlConnection(connectionString);
    }

    public IDbConnection GetConnection()
    {
        return GetConnection(_connectionString);
    }
}
