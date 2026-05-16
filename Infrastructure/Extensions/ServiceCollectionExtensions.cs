using Infrastructure.Db;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var host = builder.Configuration["DB_HOST"];
            var port = string.IsNullOrWhiteSpace(builder.Configuration["DB_PORT"])
                ? "5432"
                : builder.Configuration["DB_PORT"];
            var db = builder.Configuration["DB_NAME"];
            var user = builder.Configuration["DB_USER"];
            var pass = builder.Configuration["DB_PASSWORD"];

            var missingValues = new Dictionary<string, string?>
            {
                ["DB_HOST"] = host,
                ["DB_NAME"] = db,
                ["DB_USER"] = user,
                ["DB_PASSWORD"] = pass
            }
                .Where(x => string.IsNullOrWhiteSpace(x.Value))
                .Select(x => x.Key)
                .ToArray();

            if (missingValues.Length > 0)
            {
                throw new InvalidOperationException(
                    $"Database connection is not configured. Set ConnectionStrings:DefaultConnection or missing env values: {string.Join(", ", missingValues)}.");
            }

            connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={pass}";
        }

        builder.Services.AddDbContext<DatabaseContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory"));
        });

        return builder;
    }
}
