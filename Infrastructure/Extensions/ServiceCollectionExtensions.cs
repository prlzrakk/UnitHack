using Microsoft.AspNetCore.Builder;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        var host = builder.Configuration["DB_HOST"];
        var port = builder.Configuration["DB_PORT"];
        var db = builder.Configuration["DB_NAME"];
        var user = builder.Configuration["DB_USER"];
        var pass = builder.Configuration["DB_PASSWORD"];
        
        var connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={pass}";
        var env = builder.Environment;

        // builder.Services.AddDbContext<ProjectContext>((sp, options) =>
        // {
        //     options.UseNpgsql(connectionString, x =>
        //         x.MigrationsHistoryTable("__EFMigrationsHistory_Project"));
        // });

        return builder;
    }
}