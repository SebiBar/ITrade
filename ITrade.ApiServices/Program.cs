using DotNetEnv;
using ITrade.ApiServices.Helpers;
using ITrade.Common.Helpers;
using ITrade.DB;
using ITrade.Services.Interfaces;
using ITrade.Services.Services;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using System.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.AddNpgsqlDbContext<Context>(connectionName: "ITradeDB");

Env.Load(Path.Combine(Directory.GetCurrentDirectory(), $".env"));

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<MailJetSettings>(
    builder.Configuration.GetSection("MailJet"));

builder.Services.AddHttpClient("Mailjet", (sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<MailJetSettings>>().Value;
    client.BaseAddress = new Uri(settings.Endpoint);

    var byteArray = Encoding.ASCII.GetBytes($"{settings.Key}:{settings.Secret}");
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
});

//Service initializations here
builder.Services.AddScoped<IDatabaseSeedService, DatabaseSeedService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();


builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExceptionHandlingMiddleware>();

var app = builder.Build();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    using var scope = app.Services.CreateScope();
    var seedService = scope.ServiceProvider.GetRequiredService<IDatabaseSeedService>();
    seedService.MigrateDatabase(scope);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
