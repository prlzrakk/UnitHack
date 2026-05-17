using Api.Application.Common;
using Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder
    .LoadEnvFiles()
    .AddApiServices()
    .AddSecurityServices()
    .AddAuthorizationPolicy()
    .AddRabbitMq()
    .AddSwaggerWithAuth()
    .AddApplicationServices()
    .AddDatabase()
    .AddInfrastructureServices()
    .AddCorsPolicy()
    .AddForwardedHeaders();

var app = builder.Build();

app
    .UseConfiguredForwardedHeaders()
    .UseExceptionHandling()
    .UseSwaggerIfDevelopment()
    .UseFrontendFiles()
    .UseConfiguredCors()
    .UseAuth()
    .MapApiControllers();

app.Run();
