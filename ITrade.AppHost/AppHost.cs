var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddPostgres("db")
    .WithDataVolume()
    .WithPgWeb()
    .AddDatabase("ITradeDB");

var apiService = builder.AddProject<Projects.ITrade_ApiServices>("itrade-apiservices")
    .WithReference(db)
    .WaitFor(db);

var frontend = builder.AddContainer("itrade-frontend", "itrade-frontend")
    .WithDockerfile("../ITrade.UserClient/itrade")
    .WithHttpEndpoint(port: 80, targetPort: 80, name: "http")
    .WithEnvironment("VITE_API_URL", apiService.GetEndpoint("http"));

builder.Build().Run();
