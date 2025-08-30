using Farmundo.Demo.Api.Authorization;
using Farmundo.Demo.Api.Configurations;
using Farmundo.Demo.Api.Hubs;
using Farmundo.Demo.Api.Security;
using Farmundo.Demo.Application.Abstractions.Chat;
using Farmundo.Demo.Application.Abstractions.Persistence;
using Farmundo.Demo.Application.Abstractions.Users;
using Farmundo.Demo.Application.Services.Chat;
using Farmundo.Demo.Application.Services.Users;
using Farmundo.Demo.Domain.Models;
using Farmundo.Demo.Infrastructure.Persistence;
using Farmundo.Demo.Infrastructure.Repositories;
using HealthChecks.NpgSql;
using HealthChecks.Redis;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var config = builder.Configuration;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddMemoryCache();

services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Farmundo Demo API", Version = "v1" });
    // Manual Bearer token entry only
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// EF Core PgSQL
services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(config.GetConnectionString("Postgres"));
});

// Redis cache + SignalR backplane (optional)
var redisCnn = config.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redisCnn))
{
    services.AddStackExchangeRedisCache(o => o.Configuration = redisCnn);
    services.AddSignalR().AddStackExchangeRedis(redisCnn);
}
else
{
    services.AddDistributedMemoryCache();
    services.AddSignalR();
}

// Health checks
services.AddHealthChecks()
    .AddNpgSql(config.GetConnectionString("Postgres")!)
    .AddRedis(config.GetConnectionString("Redis")!);

// Auth: bind JwtBearerOptions from configuration (User Secrets / appsettings)
services.ConfigureOptions<JwtBearerConfigOptions>();


JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Keep only SignalR access_token support; all other options are bound from configuration
    options.Events = new JwtBearerEvents
    {
        // TODO remove for production. Just used here for testing/debugging purposes
        OnTokenValidated = context =>
        {
            var claims = context.Principal.Claims;
            // Log all claims here to debug
            var sb = new StringBuilder();
            foreach (var claim in claims)
            {
                sb.Append($"{claim.Type}:{claim.Value} , ");
            }
            var res = sb.ToString();
            return Task.CompletedTask;
        },

        OnMessageReceived = context =>
        {
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

// Custom authorization handlers
services.AddScoped<IAuthorizationHandler, SubscriptionHandler>();
services.AddScoped<IAuthorizationHandler, LocalRoleOrClaimHandler>();

services.AddAuthorization(options =>
{
    // Admin if claim role==admin OR local role admin
    options.AddPolicy("Admin", p => p.Requirements.Add(new LocalRoleOrClaimRequirement("admin")));

    // Subscription policies
    options.AddPolicy("SubscriptionTrial", p => p.Requirements.Add(new SubscriptionRequirement(SubscriptionTier.Trial)));
    options.AddPolicy("SubscriptionStandard", p => p.Requirements.Add(new SubscriptionRequirement(SubscriptionTier.Standard)));
    options.AddPolicy("SubscriptionPremium", p => p.Requirements.Add(new SubscriptionRequirement(SubscriptionTier.Premium)));
});

// Claims normalization
services.AddSingleton<IClaimsTransformation, ClaimsMappingTransformation>();

// DI registrations
services.AddScoped<IChatRepository, ChatRepository>();
services.AddScoped<IChatService, ChatService>();
services.AddScoped<IUserProfileRepository, UserProfileRepository>();
services.AddScoped<IUserProfileService, UserProfileService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.MapGet("/api/ping", () => Results.Ok(new { message = "pong" }));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
app.Run();

public partial class Program { }
