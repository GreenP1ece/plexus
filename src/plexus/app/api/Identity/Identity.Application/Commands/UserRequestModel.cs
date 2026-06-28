namespace Identity.Application.Commands;

public abstract class UserRequestModel(string email, string password)
{
    public string Email { get; } = email;

    public string Password { get; } = password;
}