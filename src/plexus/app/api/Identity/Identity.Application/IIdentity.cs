using Common.Application;
using Identity.Application.Commands;
using Identity.Application.Commands.ChangePassword;

namespace Identity.Application;

public interface IIdentity
{
    Task<Result<IUser>> Register(UserRequestModel userRequest);

    Task<Result<UserResponseModel>> Login(UserRequestModel userRequest);

    Task<Result> ChangePassword(ChangePasswordRequestModel changePasswordRequest);
}