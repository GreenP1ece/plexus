namespace Identity.Domain.Repositories;

public interface IUserSyncService
{
    Task<Guid> CreateUserAsync(
        string username,
        string firstName,
        string lastName,
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task DeleteUserAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}