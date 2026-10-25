using Orleans.Configuration;

var builder = WebApplication.CreateBuilder(args);
var db = builder.Configuration.GetConnectionString("db") ?? throw new InvalidOperationException("Missing connection string 'db'");

// Register cluster client and factory for dependency injection
builder.UseOrleansClient(options =>
{
    if (db is null)
    {
        options.UseLocalhostClustering(gatewayPort: 30000);
    }
    else
    {
        options.Configure<ClusterOptions>(o => { o.ClusterId = "bank"; o.ServiceId = "bank"; });
        options.UseAdoNetClustering(o => { o.Invariant = "Npgsql"; o.ConnectionString = db; });
    }
});

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
