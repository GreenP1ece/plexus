using Api.Configuration;
using Microsoft.OpenApi;

namespace Api.Extensions;

internal static class SwaggerExtensions
{
    public static IServiceCollection AddApiSwagger(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var keycloak = KeycloakSettings.FromConfiguration(configuration);

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Plexus API",
                Version = "v1"
            });

            options.AddSecurityDefinition(
                nameof(SecuritySchemeType.OAuth2),
                new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = keycloak.AuthorizationUrl,
                            TokenUrl = keycloak.TokenUrl,
                            Scopes = new Dictionary<string, string>
                            {
                                { "openid", "OpenID Connect" },
                                { "profile", "User profile" },
                                { "email", "Email" }
                            }
                        }
                    }
                });

            options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference(
                        nameof(SecuritySchemeType.OAuth2),
                        doc),
                    ["openid", "profile"]
                }
            });
        });

        services.AddOpenApi();

        return services;
    }

    public static WebApplication UseApiSwagger(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        var keycloak = KeycloakSettings.FromConfiguration(app.Configuration);

        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint(
                "/swagger/v1/swagger.json",
                "Plexus API v1");

            options.OAuthClientId(keycloak.ClientId);
            options.OAuthClientSecret(keycloak.ClientSecret);
            options.OAuthUsePkce();
            options.OAuthScopes("openid", "profile", "email");
        });

        app.MapOpenApi();

        return app;
    }
}