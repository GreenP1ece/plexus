namespace Identity.Application.Commands.ChangePassword;
public class ChangePasswordRequestModel(
    string userId,
    string currentPassword,
    string newPassword)
{
    public string UserId { get; } = userId;

    public string CurrentPassword { get; } = currentPassword;

    public string NewPassword { get; } = newPassword;
}