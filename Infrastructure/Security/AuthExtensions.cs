using Client.Models.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shared.Models.Enums;

namespace Infrastructure.Security;

public static class AuthPolicies
{
    public const string RefreshTokenOnly = "RefreshTokenOnly";
}

public static class AuthExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services, JwtSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var signingKey = JwtSecretProvider.CreateSigningKey(settings.SecretKey);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    RequireExpirationTime = false,
                    LifetimeValidator = (notBefore, expires, token, parameters) =>
                        JwtLifetimeValidator.Validate(notBefore, expires, token, settings),
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey
                };
            });
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.RefreshTokenOnly,
                policy => { policy.RequireClaim(TokenClaimTypes.TokenType, TokenTypeNames.Refresh); });
        });

        return services;
    }
}
