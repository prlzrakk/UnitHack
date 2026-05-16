using System.Reflection;
using Client.Models.Configs;

namespace WebApplication1.Application.Common;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder LoadEnvFiles(this WebApplicationBuilder builder)
    {
        DotNetEnv.Env.NoClobber().Load("../.env");
        DotNetEnv.Env.NoClobber().Load();
        builder.Configuration.AddEnvironmentVariables();

        return builder;
    }
    
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddMediatR(cfg =>
        {
            var mediatRConfig = builder.Configuration.GetSection("Licenses").Get<MediatRConfig>();
            if (mediatRConfig is not null)
                cfg.LicenseKey = mediatRConfig.LicenseKey;
            cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
        });

        return builder;
    }

    public static WebApplicationBuilder AddSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
        });

        return builder;
    }

    public static WebApplicationBuilder AddInfrastructureServices(this WebApplicationBuilder builder)
    {
        // builder.Services.AddScoped<IProjectRepository, ProjectRepository>();

        return builder;
    }
    public static WebApplicationBuilder AddAuthorizationPolicy(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("RequireEmail", policy =>
                policy.RequireClaim("email"));

        return builder;
    }

}