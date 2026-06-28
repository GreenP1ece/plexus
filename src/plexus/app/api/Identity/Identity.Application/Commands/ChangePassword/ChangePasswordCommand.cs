

using Common.Application;
using Common.Application.Contracts;
using Mediator;

namespace Identity.Application.Commands.ChangePassword;
public class ChangePasswordCommand : IRequest<Result>
{
    public string CurrentPassword { get; set; } = default!;

    public string NewPassword { get; set; } = default!;

    public class ChangePasswordCommandHandler(
        IIdentity identity,
        ICurrentUser currentUser) : IRequestHandler<ChangePasswordCommand, Result>
    {

        public async ValueTask<Result> Handle(
            ChangePasswordCommand request,
            CancellationToken cancellationToken)
            => await identity.ChangePassword(new ChangePasswordRequestModel(
                currentUser.UserId,
                request.CurrentPassword,
                request.NewPassword));
    }
}