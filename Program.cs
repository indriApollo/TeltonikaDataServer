using TeltonikaDataServer;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<UdpServer>();
    })
    .Build();

await host.RunAsync();
