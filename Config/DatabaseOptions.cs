namespace TeltonikaDataServer.Config;

public class DatabaseOptions
{
    public const string Database = "Database";

    public string ConnectionString { get; set; } = null!;
}
