using Common.Domain;
using Common.Domain.Models;

namespace Identity.Domain.Users;

public class User : Entity, IAggregateRoot
{
    public string KeycloakId { get; private set; } = default!;
    public string Username { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public Email Email { get; private set; } = default!;

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private User() { }

    private User(
        string keycloakId,
        string username,
        string firstName,
        string lastName,
        Email email)
    {
        KeycloakId = keycloakId;
        Username = username;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        CreatedAt = DateTime.UtcNow;
    }

    public static User CreateFromKeycloak(
        string keycloakId,
        string username,
        string firstName,
        string lastName,
        Email email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keycloakId);
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        var user = new User(keycloakId, username, firstName, lastName, email);
        //user.RaiseEvent(new UserCreatedEvent(user.Id));
        return user;
    }

    public void UpdateProfile(string firstName, string lastName, Email email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        if (DeletedAt is not null) return;
        DeletedAt = DateTime.UtcNow;
        //RaiseEvent(new UserDeletedEvent(Id));
    }
}