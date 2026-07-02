namespace Identity.Infrastructure.Configuration;

public sealed class KeycloakAdminOptions
{
    public const string SectionName = "KeycloakAdmin";

    public string BaseUrl { get; init; } = default!;
    public string Realm { get; init; } = default!;
    public string AuthRealm { get; init; } = "master";
    public string ClientId { get; init; } = default!;
    public string ClientSecret { get; init; } = default!;
}