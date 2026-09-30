using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Transaction.WebApi.Security;

internal static class AuthenticationRegistration
{
    /// <summary>JWT bearer authentication against the signing keys published by Identity.</summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services
            .AddOptions<JwtAuthenticationOptions>()
            .BindConfiguration(JwtAuthenticationOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();
        services.AddAuthorization();

        return services;
    }

    private sealed class ConfigureJwtBearerOptions(IOptions<JwtAuthenticationOptions> authenticationOptions)
        : IPostConfigureOptions<JwtBearerOptions>
    {
        public void PostConfigure(string? name, JwtBearerOptions options)
        {
            var authentication = authenticationOptions.Value;

            // Keep the token's own claim names (e.g. "sub"), without the legacy ClaimTypes remapping.
            options.MapInboundClaims = false;
            options.RequireHttpsMetadata = authentication.RequireHttpsMetadata;
            options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                authentication.JwksUrl,
                new JwksConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = authentication.RequireHttpsMetadata });

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = authentication.Issuer,
                ValidateAudience = true,
                ValidAudience = authentication.Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
            };
        }
    }
}
