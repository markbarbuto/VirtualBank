var builder = WebApplication.CreateBuilder(args);

// Register cluster client and factory for dependency injection
builder.UseOrleansClient(options => 
    options.UseLocalhostClustering(gatewayPort: 30000));

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
