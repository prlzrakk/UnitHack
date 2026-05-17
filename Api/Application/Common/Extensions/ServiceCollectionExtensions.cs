using System.Reflection;
using System.Text.Json.Serialization;
using Api.Middleware;
using Client.Models.Configs;
using FluentValidation;
using Infrastructure.Extensions;
using Infrastructure.RabbitMq;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Security;
using Infrastructure.Security.Interfaces;
using Infrastructure.Workers;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
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

    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        builder.Services.AddProblemDetails();

        return builder;
    }

    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddMediatR(cfg =>
        {
            var mediatRConfig = builder.Configuration
                .GetSection("Licenses")
                .Get<MediatRConfig>();

            if (mediatRConfig is not null)
                cfg.LicenseKey = mediatRConfig.LicenseKey;

            cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
        });

        builder.Services.AddValidatorsFromAssemblyContaining<Program>();
        builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return builder;
    }

    public static WebApplicationBuilder AddSecurityServices(this WebApplicationBuilder builder)
    {
        var jwtSettings = JwtSettingsResolver.Resolve(builder.Configuration);

        builder.Services.Configure<JwtSettings>(
            builder.Configuration.GetSection("JwtSettings"));

        builder.Services.AddSingleton<ITokenService, JwtTokenService>();
        builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        builder.Services.AddAuth(jwtSettings);

        return builder;
    }

    public static WebApplicationBuilder AddRabbitMq(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<RabbitMqOptions>(
            builder.Configuration.GetSection("RabbitMq"));
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
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IKanbanRepository, KanbanRepository>();
        builder.Services.AddScoped<IKanbanColumnRepository, KanbanColumnRepository>();
        builder.Services.AddScoped<IKanbanTaskRepository, KanbanTaskRepository>();
        builder.Services.AddScoped<ITagRepository, TagRepository>();
        builder.Services.AddScoped<ITaskTagRepository, TaskTagRepository>();
        builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
        builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
        builder.Services.AddScoped<ITeamRepository, TeamRepository>();
        builder.Services.AddScoped<ITeamMemberRepository, TeamMemberRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWorkRepository>();
        builder.Services.AddHostedService<OutboxWorker>();
        
        return builder;
    }

    public static WebApplicationBuilder AddAuthorizationPolicy(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("RequireUserId", policy =>
                policy.RequireClaim("user_id"));

        return builder;
    }

    public static WebApplicationBuilder AddForwardedHeaders(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto;
        });

        return builder;
    }

    public static WebApplicationBuilder AddCorsPolicy(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("FrontendDev", policy =>
            {
                policy
                    .WithOrigins("http://localhost:63342")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return builder;
    }

    public static WebApplication UseExceptionHandling(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        return app;
    }

    public static WebApplication UseConfiguredForwardedHeaders(this WebApplication app)
    {
        app.UseForwardedHeaders();

        return app;
    }

    public static WebApplication UseConfiguredCors(this WebApplication app)
    {
        app.UseCors("FrontendDev");

        return app;
    }

    public static WebApplication UseSwaggerIfDevelopment(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }

    public static WebApplication UseFrontendFiles(this WebApplication app)
    {
        app.UseDefaultFiles();
        app.UseStaticFiles();

        return app;
    }

    public static WebApplication UseAuth(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    public static WebApplication MapApiControllers(this WebApplication app)
    {
        app.MapControllers();

        return app;
    }
}
