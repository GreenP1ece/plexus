using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Identity.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Services;

internal sealed class KeycloakAdminClient
(
    HttpClient httpClient,
    IOptions<KeycloakAdminOptions> options
) : IKeycloakAdminClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly KeycloakAdminOptions _options = options.Value;

    public async Task<string> CreateAsync(CreateKeycloakUserRequest request, CancellationToken ct = default)
    {
        var token = await GetAdminTokenAsync(ct);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new
        {
            username = request.Username,
            email = request.Email,
            firstName = request.FirstName,
            lastName = request.LastName,
            enabled = true,
            credentials = new[]
            {
                new { type = "password", value = request.Password, temporary = false }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"admin/realms/{_options.Realm}/users", payload, ct);

        response.EnsureSuccessStatusCode();

        var location = response.Headers.Location?.ToString();
        return location?.Split('/').LastOrDefault() ?? string.Empty;
    }

    public async Task<bool> DeleteAsync(string userId, CancellationToken cancellationToken = default)
    {
        var token = await GetAdminTokenAsync(cancellationToken);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.DeleteAsync(
            $"admin/realms/{_options.Realm}/users/{userId}", cancellationToken);

        response.EnsureSuccessStatusCode();

        return true;
    }

    public async Task<KeycloakUserDto?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var token = await GetAdminTokenAsync(cancellationToken);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync(
            $"admin/realms/{_options.Realm}/users/{userId}", cancellationToken);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        return json.Deserialize<KeycloakUserDto>();
    }

    private async Task<string> GetAdminTokenAsync(CancellationToken ct)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret
        });

        var response = await _httpClient.PostAsync(
            $"realms/{_options.AuthRealm}/protocol/openid-connect/token", 
            content, 
            ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        return json.GetProperty("access_token").GetString()!;
    }
}