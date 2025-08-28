using HealthChecks.NpgSql;
using HealthChecks.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Farmundo.Demo.Infrastructure.Persistence;
using Farmundo.Demo.Infrastructure.Repositories;
using Farmundo.Demo.Application.Abstractions.Chat;
using Farmundo.Demo.Application.Services.Chat;
using Farmundo.Demo.Application.Abstractions.Persistence;
using Farmundo.Demo.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var config = builder.Configuration;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

// EF Core PgSQL
services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(config.GetConnectionString("Postgres"));
});

// Redis cache + SignalR backplane
services.AddStackExchangeRedisCache(o =>
{
    o.Configuration = config.GetConnectionString("Redis");
});
services.AddSignalR().AddStackExchangeRedis(config.GetConnectionString("Redis"));

// Health checks
services.AddHealthChecks()
    .AddNpgSql(config.GetConnectionString("Postgres")!)
    .AddRedis(config.GetConnectionString("Redis")!);

// Auth: JWT bearer for either Azure B2C or Cognito via config
var authProvider = config.GetValue<string>("Auth:Provider");
services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    if (string.Equals(authProvider, "AzureB2C", StringComparison.OrdinalIgnoreCase))
    {
        options.Authority = config["AzureAdB2C:Authority"];
        options.Audience = config["AzureAdB2C:ClientId"];
        options.TokenValidationParameters = new() { ValidIssuer = options.Authority };
    }
    else if (string.Equals(authProvider, "Cognito", StringComparison.OrdinalIgnoreCase))
    {
        var region = config["Cognito:Region"];
        var poolId = config["Cognito:UserPoolId"];
        var clientId = config["Cognito:ClientId"];
        options.Authority = $"https://cognito-idp.{region}.amazonaws.com/{poolId}";
        options.Audience = clientId;
    }
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Allow SignalR access token via query string for WebSockets
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});
services.AddAuthorization(options =>
{
    options.AddPolicy("Farmer", p => p.RequireClaim("role", "farmer"));
    options.AddPolicy("Buyer", p => p.RequireClaim("role", "buyer"));
    options.AddPolicy("Admin", p => p.RequireClaim("role", "admin"));
});

// DI registrations
services.AddScoped<IChatRepository, ChatRepository>();
services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.MapGet("/api/ping", () => Results.Ok(new { message = "pong" }));

app.Run();

public partial class Program { }
