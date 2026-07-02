using Microsoft.Extensions.Configuration;
using Common.Web.Extensions;

namespace Common.Web.Configuration;

public sealed class KeycloakSettings
{
    public required string Authority { get; init; }
    public required string MetadataAddress { get; init; }
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string Audience { get; init; }
    public required string Issuer { get; init; }

    public Uri AuthorizationUrl =>
        new($"{Authority.TrimEnd('/')}/protocol/openid-connect/auth");

    public Uri TokenUrl =>
        new($"{Authority.TrimEnd('/')}/protocol/openid-connect/token");

    public static KeycloakSettings FromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection("Keycloak");

        return new KeycloakSettings
        {
            Authority = section.GetRequiredValue("Authority"),
            MetadataAddress = section.GetRequiredValue("MetadataAddress"),
            ClientId = section.GetRequiredValue("ClientId"),
            ClientSecret = section.GetRequiredValue("ClientSecret"),
            Audience = section.GetRequiredValue("Audience"),
            Issuer = section.GetRequiredValue("Issuer")
        };
    }
}