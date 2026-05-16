using System.Text.Json.Serialization;
using Api.Application.Common;
using Api.Application.Common.Exceptions;
using FluentValidation;
using Infrastructure.Extensions;
using Infrastructure.Security;
using Infrastructure.Security.Interfaces;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.LoadEnvFiles();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddProblemDetails();

var jwtSettings = JwtSettingsResolver.Resolve(builder.Configuration);
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddAuth(jwtSettings);

builder
    .AddAuthorizationPolicy()
    .AddSwaggerWithAuth()
    .AddApplicationServices()
    .AddDatabase()
    .AddInfrastructureServices();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

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

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (ApiException exception)
    {
        await WriteProblemDetailsAsync(
            context,
            exception.StatusCode,
            exception.ErrorCode,
            exception.Message,
            exception.ErrorCode);
    }
    catch (ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                x => x.Key,
                x => x.Select(error => error.ErrorMessage).ToArray());

        await WriteProblemDetailsAsync(
            context,
            StatusCodes.Status400BadRequest,
            "validation_failed",
            "Request validation failed",
            "validation_failed",
            errors);
    }
    catch (UnauthorizedAccessException exception)
    {
        await WriteProblemDetailsAsync(
            context,
            StatusCodes.Status401Unauthorized,
            "unauthorized",
            exception.Message,
            "unauthorized");
    }
});

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("FrontendDev");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

static async Task WriteProblemDetailsAsync(
    HttpContext context,
    int statusCode,
    string title,
    string detail,
    string errorCode,
    object? errors = null)
{
    if (context.Response.HasStarted)
        throw new InvalidOperationException("The response has already started.");

    context.Response.Clear();
    context.Response.StatusCode = statusCode;
    context.Response.ContentType = "application/problem+json";

    var problem = new ProblemDetails
    {
        Status = statusCode,
        Title = title,
        Detail = detail,
        Instance = context.Request.Path
    };
    problem.Extensions["errorCode"] = errorCode;
    if (errors is not null)
        problem.Extensions["errors"] = errors;

    await context.Response.WriteAsJsonAsync(problem);
}
