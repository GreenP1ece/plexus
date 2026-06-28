using Common.Application;
using Mediator;

namespace Identity.Application.Commands.LoginUser;

public class LoginUserCommand(string email, string password)
    : UserRequestModel(email, password), IRequest<Result<UserResponseModel>>
{
    public class LoginUserCommandHandler(IIdentity identity)
        : IRequestHandler<LoginUserCommand, Result<UserResponseModel>>
    {
        private readonly IIdentity _identity = identity;

        public async ValueTask<Result<UserResponseModel>> Handle(
            LoginUserCommand request,
            CancellationToken cancellationToken)
            => await _identity.Login(request);
    }
}