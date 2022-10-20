using System.Data;
using Npgsql;
using Microsoft.Extensions.Options;
using TeltonikaDataServer.Config;

namespace TeltonikaDataServer.Services;

public class Database
{
    private readonly string _connectionString;
    
    public Database(IOptions<DatabaseOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
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
