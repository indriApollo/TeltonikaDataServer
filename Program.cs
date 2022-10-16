using TeltonikaDataServer;
using TeltonikaDataServer.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<Database>();
        services.AddSingleton<TelematicsDataRepository>();
        services.AddSingleton<TeltonikaDataHandler>();
        services.AddHostedService<UdpServer>();
    })
    .Build();

await host.RunAsync();
