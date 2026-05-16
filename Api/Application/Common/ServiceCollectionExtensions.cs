using System.Reflection;
using Client.Models.Configs;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Repositories.Mocks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;

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
        builder.Services.AddMediatR(cfg =>
        {
            var mediatRConfig = builder.Configuration.GetSection("Licenses").Get<MediatRConfig>();
            if (mediatRConfig is not null)
                cfg.LicenseKey = mediatRConfig.LicenseKey;
            cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
        });

        return builder;
    }

    public static WebApplicationBuilder AddSwaggerWithAuth(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT",
                Description = "Paste JWT token here. The Authorization header will be sent as: Bearer {token}"
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                { new OpenApiSecuritySchemeReference("Bearer", document), [] }
            });
        });

        return builder;
    }

    public static WebApplicationBuilder AddInfrastructureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserRepository, UserRepositoryMock>();
        builder.Services.AddScoped<IKanbanRepository, MockKanbanRepository>();
        builder.Services.AddScoped<IKanbanColumnRepository, MockKanbanColumnRepository>();
        builder.Services.AddScoped<IKanbanTaskRepository, MockKanbanTaskRepository>();
        builder.Services.AddScoped<IProjectRepository, MockProjectRepository>();
        builder.Services.AddScoped<ITeamRepository, MockTeamRepository>();
        builder.Services.AddSingleton<MockDataStore>();
        builder.Services.AddScoped<ITeamMemberRepository, TeamMemberRepositoryMock>();
        builder.Services.AddScoped<IUnitOfWork, MockUnitOfWork>();

        return builder;
    }

    public static WebApplicationBuilder AddAuthorizationPolicy(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("RequireUserId", policy =>
                policy.RequireClaim("user_id"));

        return builder;
    }
}
