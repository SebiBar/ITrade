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

static string? FirstNonEmpty(params string?[] values)
{
    foreach (var value in values)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }
    }

    return null;
}

var databaseMigrateOnStartup = FirstNonEmpty(
    builder.Configuration["Database:MigrateOnStartup"],
    Environment.GetEnvironmentVariable("Database__MigrateOnStartup"),
    Environment.GetEnvironmentVariable("DATABASE__MIGRATEONSTARTUP"));

var urlsApiBase = FirstNonEmpty(
    builder.Configuration["Urls:ApiBase"],
    Environment.GetEnvironmentVariable("Urls__ApiBase"),
    Environment.GetEnvironmentVariable("URLS__APIBASE"));

var jwtSecret = FirstNonEmpty(
    builder.Configuration["Jwt:Secret"],
    Environment.GetEnvironmentVariable("Jwt__Secret"),
    Environment.GetEnvironmentVariable("JWT__SECRET"),
    Environment.GetEnvironmentVariable("JWT_SECRET"));

var mailJetKey = FirstNonEmpty(
    builder.Configuration["MailJet:Key"],
    Environment.GetEnvironmentVariable("MailJet__Key"),
    Environment.GetEnvironmentVariable("MAILJET__KEY"),
    Environment.GetEnvironmentVariable("MAILJET_KEY"));

var mailJetSecret = FirstNonEmpty(
    builder.Configuration["MailJet:Secret"],
    Environment.GetEnvironmentVariable("MailJet__Secret"),
    Environment.GetEnvironmentVariable("MAILJET__SECRET"),
    Environment.GetEnvironmentVariable("MAILJET_SECRET"));

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