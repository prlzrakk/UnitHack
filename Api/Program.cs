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
    .AddForwardedHeaders()
    .AddSignalR();

var app = builder.Build();

app
    .InitializeDatabase()
    .UseConfiguredForwardedHeaders()
    .UseExceptionHandling()
    .UseSwaggerIfDevelopment()
    .UseFrontendFiles()
    .UseConfiguredCors()
    .UseAuth()
    .MapApiControllers()
    .MapNotificationsHub();

app.Run();
