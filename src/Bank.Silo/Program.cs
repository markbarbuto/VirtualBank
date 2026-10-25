using System.Net;
using Orleans.Configuration;

var builder = Host.CreateApplicationBuilder(args);

// Ports come from the command line so we can run several silos on one machine.
var siloPort = builder.Configuration.GetValue("siloPort", 11111);
var gatewayPort = builder.Configuration.GetValue("gatewayPort", 30000);

builder.UseOrleans(silo =>
{
    // Dev: Use localhost clustering so we can run multiple silos on the same machine.
    // In Prod: Use a proper clustering provider (e.g. Azure Table, Redis, Kubernetes, etc.) and configure it here.
    silo.UseLocalhostClustering(
        siloPort: siloPort,
        gatewayPort: gatewayPort,
        primarySiloEndpoint: new IPEndPoint(IPAddress.Loopback, 11111));

    silo.AddMemoryGrainStorage("bank");

    // Deactivate idle grains quickly for the sake of the tutorial
    // -> so we can watch it happen (default is much longer).
    silo.Configure<GrainCollectionOptions>(o =>
    {
        o.CollectionQuantum = TimeSpan.FromSeconds(10);
        o.CollectionAge = TimeSpan.FromSeconds(30);
    });
});

builder.Build().Run();