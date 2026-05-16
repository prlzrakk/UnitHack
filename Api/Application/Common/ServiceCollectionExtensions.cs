using System.Reflection;
using Client.Models.Configs;
using FluentValidation;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Repositories.Mocks;
using MediatR;

namespace Api.Application.Common;

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
        var applicationAssembly = typeof(ServiceCollectionExtensions).Assembly;
        
        builder.Services.AddMediatR(cfg =>
        {
            var mediatRConfig = builder.Configuration.GetSection("Licenses").Get<MediatRConfig>();
            if (mediatRConfig is not null)
                cfg.LicenseKey = mediatRConfig.LicenseKey;
            cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
        });
        
        builder.Services.AddValidatorsFromAssembly(applicationAssembly);
        
        builder.Services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        return builder;
    }

    public static WebApplicationBuilder AddSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        });

        return builder;
    }

    public static WebApplicationBuilder AddInfrastructureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserRepository, UserRepositoryMock>();

        return builder;
    }
    public static WebApplicationBuilder AddAuthorizationPolicy(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("RequireEmail", policy =>
                policy.RequireClaim("email"));

        // builder.Services.AddScoped<IProjectRepository, ProjectRepository>();

        return builder;
    }
}