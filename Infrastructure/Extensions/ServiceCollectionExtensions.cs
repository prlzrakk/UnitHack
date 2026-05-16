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
