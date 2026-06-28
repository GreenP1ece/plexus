using System.Security.Claims;
using System.Text.Json;

namespace Api.Extensions;

internal static class AuthenticationExtensions
{
    public static IServiceCollection AddTokenAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var keycloak = KeycloakSettings.FromConfiguration(configuration);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MetadataAddress = keycloak.MetadataAddress;

                options.Authority = keycloak.Authority;

                options.Audience = keycloak.Audience;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = keycloak.Issuer,
                    ValidAudience = keycloak.Audience,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };

                options.RequireHttpsMetadata = !environment.IsDevelopment();

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = AddKeycloakRealmRoles
                };
            });

        return services;
    }

    public static IServiceCollection AddApiAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorization();

        return services;
    }

    private static Task AddKeycloakRealmRoles(TokenValidatedContext context)
    {
        var principal = context.Principal;

        if (principal?.Identity is not ClaimsIdentity claimsIdentity)
        {
            return Task.CompletedTask;
        }

        var realmAccess = principal.FindFirst("realm_access")?.Value;

        if (string.IsNullOrWhiteSpace(realmAccess))
        {
            return Task.CompletedTask;
        }

        try
        {
            using var parsed = JsonDocument.Parse(realmAccess);

            if (!parsed.RootElement.TryGetProperty("roles", out var roles))
            {
                return Task.CompletedTask;
            }

            if (roles.ValueKind is not JsonValueKind.Array)
            {
                return Task.CompletedTask;
            }

            foreach (var role in roles.EnumerateArray())
            {
                var roleName = role.GetString();

                if (string.IsNullOrWhiteSpace(roleName))
                {
                    continue;
                }

                var alreadyExists = claimsIdentity.Claims.Any(c =>
                    c.Type == ClaimTypes.Role &&
                    c.Value == roleName);

                if (!alreadyExists)
                {
                    claimsIdentity.AddClaim(
                        new Claim(ClaimTypes.Role, roleName));
                }
            }
        }
        catch (JsonException)
        {
        }

        return Task.CompletedTask;
    }
}