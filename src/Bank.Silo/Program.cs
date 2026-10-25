using System.Net;
using Orleans.Configuration;
using Orleans.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Ports come from the command line so we can run several silos on one machine.
var siloPort = builder.Configuration.GetValue("siloPort", 11111);
var gatewayPort = builder.Configuration.GetValue("gatewayPort", 30000);
var db = builder.Configuration.GetConnectionString("db") ?? throw new InvalidOperationException("Missing connection string 'db'");

builder.UseOrleans(silo =>
{
    // Dev: Use localhost clustering so we can run multiple silos on the same machine.
    // In Prod: Use a proper clustering provider (e.g. Azure Table, Redis, Kubernetes, etc.) and configure it here.
    if (db is null)
    {
        silo.UseLocalhostClustering(
            siloPort: siloPort,
            gatewayPort: gatewayPort,
            primarySiloEndpoint: new IPEndPoint(IPAddress.Loopback, 11111));

        silo.AddMemoryGrainStorage("bank");
    }
    else
    {
        silo.Configure<ClusterOptions>(o => { o.ClusterId = "bank"; o.ServiceId = "bank"; });
        silo.ConfigureEndpoints(siloPort: siloPort, gatewayPort: gatewayPort, listenOnAnyHostAddress: true);
        silo.UseAdoNetClustering(o => { o.Invariant = "Npgsql"; o.ConnectionString = db; });
        silo.AddAdoNetGrainStorage("bank", o => { o.Invariant = "Npgsql"; o.ConnectionString = db; });
    }
    
    // Deactivate idle grains quickly for the sake of the tutorial
    // -> so we can watch it happen (default is much longer).
    silo.Configure<GrainCollectionOptions>(o =>
    {
        o.CollectionQuantum = TimeSpan.FromSeconds(10);
        o.CollectionAge = TimeSpan.FromSeconds(30);
    });
});

builder.Build().Run();