using Api.Application.Common;
using Infrastructure.Extensions;
using Infrastructure.Security;
using Infrastructure.Security.Interfaces;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.LoadEnvFiles();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

var jwtSettings = JwtSettingsResolver.Resolve(builder.Configuration);
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddAuth(jwtSettings);

builder
    .AddAuthorizationPolicy()
    .AddSwagger()
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
