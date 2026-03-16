var builder = DistributedApplication.CreateBuilder(args);

var pgUser = builder.AddParameter("pg-user", secret: true);
var pgPass = builder.AddParameter("pg-pass", secret: true);

var postgres = builder.AddAzurePostgresFlexibleServer("db")
    .WithPasswordAuthentication(pgUser, pgPass)
    .RunAsContainer(localContainer =>
    {
        localContainer.WithDataVolume();

        if (builder.ExecutionContext.IsRunMode)
        {
            localContainer.WithPgWeb();
        }
    });

var db = postgres.AddDatabase("ITradeDB");

var apiService = builder.AddProject<Projects.ITrade_ApiServices>("itrade-apiservices")
    .WithReference(db)
    .WaitFor(db);

var frontend = builder.AddNpmApp("itrade-frontend", "../ITrade.UserClient/itrade", "dev")
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithEnvironment("VITE_API_URL", apiService.GetEndpoint("https"))
    .WithHttpEndpoint(targetPort: 80, name: "http")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();