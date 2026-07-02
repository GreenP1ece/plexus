
using Identity.Domain.Models;
using Identity.Domain.Repositories;
using Identity.Infrastructure.Persistence;
namespace Identity.Infrastructure.Services;

internal sealed class UserSyncService(
    IUserRepository userRepository,
    IKeycloakAdminClient keycloakAdminClient,
    IdentityDbContext dbContext) : IUserSyncService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IKeycloakAdminClient _keycloakAdminClient = keycloakAdminClient;
    private readonly IdentityDbContext _dbContext = dbContext;

    public async Task<Guid> CreateUserAsync(
        string username,
        string firstName,
        string lastName,
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var keycloakUserId = await _keycloakAdminClient.CreateAsync(
            new CreateKeycloakUserRequest
            {
                Username = username,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = password
            },
            cancellationToken);

        var user = User.CreateFromKeycloak(
            keycloakUserId,
            username,
            firstName,
            lastName,
            Email.Create(email));

        await _userRepository.AddAsync(user, cancellationToken);
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        return user.Id;
    }

    public async Task DeleteUserAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"User '{id}' not found.");

        await _keycloakAdminClient.DeleteAsync(user.KeycloakId, cancellationToken);

        user.SoftDelete();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}