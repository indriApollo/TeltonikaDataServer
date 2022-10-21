namespace TeltonikaDataServer.Config;

public class DatabaseOptions
{
    public const string Database = "Database";

    public string ConnectionString { get; set; } = null!;
    public long DbCacheSize { get; set; } = 1000;
}
