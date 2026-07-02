namespace Identity.Infrastructure.Services;

public interface IKeycloakAdminClient
{
    Task<KeycloakUserDto?> GetByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<string> CreateAsync(CreateKeycloakUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string userId, CancellationToken cancellationToken = default);
}
public sealed class KeycloakUserDto
{
    public string Id { get; init; } = default!;
    public string Username { get; init; } = default!;
    public string? Email { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public bool Enabled { get; init; }
}

public sealed class CreateKeycloakUserRequest
{
    public string Username { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string Password { get; init; } = default!;
}
