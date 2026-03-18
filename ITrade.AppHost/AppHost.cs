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

var databaseMigrateOnStartup = builder.Configuration["Database:MigrateOnStartup"];
var urlsApiBase = builder.Configuration["Urls:ApiBase"];
var jwtSecret = builder.Configuration["Jwt:Secret"];
var mailJetKey = builder.Configuration["MailJet:Key"];
var mailJetSecret = builder.Configuration["MailJet:Secret"];

var apiService = builder.AddProject<Projects.ITrade_ApiServices>("itrade-apiservices")
    .WithReference(db)
    .WaitFor(db);

if (!string.IsNullOrWhiteSpace(databaseMigrateOnStartup))
{
    apiService = apiService.WithEnvironment("Database__MigrateOnStartup", databaseMigrateOnStartup);
}

if (!string.IsNullOrWhiteSpace(urlsApiBase))
{
    apiService = apiService.WithEnvironment("Urls__ApiBase", urlsApiBase);
}

if (!string.IsNullOrWhiteSpace(jwtSecret))
{
    apiService = apiService.WithEnvironment("Jwt__Secret", jwtSecret);
}

if (!string.IsNullOrWhiteSpace(mailJetKey))
{
    apiService = apiService.WithEnvironment("MailJet__Key", mailJetKey);
}

if (!string.IsNullOrWhiteSpace(mailJetSecret))
{
    apiService = apiService.WithEnvironment("MailJet__Secret", mailJetSecret);
}

var frontend = builder.AddNpmApp("itrade-frontend", "../ITrade.UserClient/itrade", "dev")
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithEnvironment("BACKEND_URL", apiService.GetEndpoint("https"));

if (builder.ExecutionContext.IsRunMode)
{
    // Let Aspire allocate a free local dev port and pass it to Vite via PORT.
    frontend = frontend.WithHttpEndpoint(name: "http", env: "PORT");
}
else
{
    // In container/publish mode the nginx image listens on port 80.
    frontend = frontend.WithHttpEndpoint(targetPort: 80, name: "http");
}

frontend = frontend
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();