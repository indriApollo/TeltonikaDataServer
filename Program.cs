using TeltonikaDataServer;
using TeltonikaDataServer.Config;
using TeltonikaDataServer.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<DatabaseOptions>(context.Configuration.GetSection(DatabaseOptions.Database));
        services.Configure<ServerOptions>(context.Configuration.GetSection(ServerOptions.Server));
        
        services.AddSingleton<Database>();
        services.AddSingleton<TelematicsDataRepository>();
        services.AddSingleton<TeltonikaDataHandler>();
        services.AddHostedService<UdpServer>();
    })
    .Build();

await host.RunAsync();
