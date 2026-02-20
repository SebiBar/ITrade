var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddPostgres("db")
    .WithDataVolume()
    .WithPgWeb()
    .AddDatabase("ITradeDB");

var apiService = builder.AddProject<Projects.ITrade_ApiServices>("itrade-apiservices")
    .WithReference(db)
    .WaitFor(db);

var frontend = builder.AddNpmApp("itrade-frontend", "../ITrade.UserClient/itrade", "dev")
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithEnvironment("VITE_API_URL", apiService.GetEndpoint("https"))
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
