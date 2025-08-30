using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Farmundo.Demo.Infrastructure.Persistence.DesignTime;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string ApiUserSecretsId = "91d68bf1-ba94-4f46-84b6-b03bcc032835";

    public AppDbContext CreateDbContext(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .AddUserSecrets(ApiUserSecretsId);

        var config = builder.Build();

        var cs = config.GetConnectionString("Postgres")
                 ?? GetConnectionStringFromSecretsFile(ApiUserSecretsId)
                 ?? "Host=localhost;Port=5432;Database=farmundo_demo;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(cs);
        return new AppDbContext(optionsBuilder.Options);
    }

    private static string? GetConnectionStringFromSecretsFile(string id)
    {
        try
        {
            string? basePath = null;
            if (OperatingSystem.IsWindows())
            {
                basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Microsoft", "UserSecrets", id);
            }
            else
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                basePath = Path.Combine(home, ".microsoft", "usersecrets", id);
            }
            var path = Path.Combine(basePath, "secrets.json");
            if (!File.Exists(path)) return null;
            using var s = File.OpenRead(path);
            var doc = JsonDocument.Parse(s);
            if (doc.RootElement.TryGetProperty("ConnectionStrings:Postgres", out var val))
                return val.GetString();
            if (doc.RootElement.TryGetProperty("ConnectionStrings", out var csObj) && csObj.TryGetProperty("Postgres", out var pg))
                return pg.GetString();
            return null;
        }
        catch { return null; }
    }
}
