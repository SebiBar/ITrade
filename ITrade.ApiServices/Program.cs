using DotNetEnv;
using ITrade.ApiServices.Helpers;
using ITrade.Common.Helpers;
using ITrade.DB;
using ITrade.DB.Entities;
using ITrade.Services.Interfaces;
using ITrade.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.AddNpgsqlDbContext<Context>(connectionName: "ITradeDB");
builder.Services.AddHttpContextAccessor();

Env.Load(Path.Combine(Directory.GetCurrentDirectory(), $".env"));

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<UrlSettings>(
    builder.Configuration.GetSection("Urls"));

builder.Services.Configure<TokenSettings>(
    builder.Configuration.GetSection("Token"));

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.Configure<MailJetSettings>(
    builder.Configuration.GetSection("MailJet"));

builder.Services.Configure<TemplateSettings>(
    builder.Configuration.GetSection("Templates"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSection = builder.Configuration.GetSection("Jwt");
    var jwt = jwtSection.Get<JwtSettings>()!;
    options.RequireHttpsMetadata = true;
    options.SaveToken = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),

        ValidateIssuer = true,
        ValidIssuer = jwt.Issuer,

        ValidateAudience = true,
        ValidAudience = jwt.Audience,

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,

        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role
    };
});

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
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<ITemplateService, TemplateService>();


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
