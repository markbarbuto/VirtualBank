var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Register cluster client and factory for dependency injection
builder.UseOrleansClient(options => 
    options.UseLocalhostClustering(gatewayPort: 30000));

//app.MapGet("/", () => "Hello World!");

app.Run();
