using Common.Application;
using Mediator;

namespace Identity.Application.Commands.RegisterUser;

public class RegisterUserCommand(
    string email,
    string password,
    string confirmPassword) : UserRequestModel(email, password), IRequest<Result>
{
    public string ConfirmPassword { get; } = confirmPassword;

    public class RegisterUserCommandHandler(IIdentity identity) 
        : IRequestHandler<RegisterUserCommand, Result>
    {
        private readonly IIdentity _identity = identity;

        public async ValueTask<Result> Handle(
            RegisterUserCommand request,
            CancellationToken cancellationToken)
            => await _identity.Register(request);
    }
}