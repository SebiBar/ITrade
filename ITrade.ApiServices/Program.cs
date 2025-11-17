using ITrade.ApiServices.Helpers;
using ITrade.DB;
using DotNetEnv;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.AddNpgsqlDbContext<Context>(connectionName: "ITradeDB");

Env.Load(Path.Combine(Directory.GetCurrentDirectory(), $".env"));

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

//Service initializations here
builder.Services.AddScoped<IDatabaseSeedService, DatabaseSeedService>();

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
