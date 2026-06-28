namespace Identity.Application.Commands;
public class UserResponseModel(string token)
{
    public string Token { get; } = token;
}