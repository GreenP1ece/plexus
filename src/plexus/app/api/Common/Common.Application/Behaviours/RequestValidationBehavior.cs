using FluentValidation;
using Mediator;

namespace Common.Application.Behaviours;
public class RequestValidationBehavior<TMessage, TResponse>
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private readonly IEnumerable<IValidator<TMessage>> validators;

    public RequestValidationBehavior(IEnumerable<IValidator<TMessage>> validators)
        => this.validators = validators;

    public async ValueTask<TResponse> Handle(
        TMessage  message,
        CancellationToken ct,
        MessageHandlerDelegate<TMessage, TResponse> next)
    {
        var context = new ValidationContext<TMessage>(message);

        var results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, ct)));

        var errors = results
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (errors.Count != 0)
        {
            throw new ModelValidationException(errors);
        }

        return await next(message, ct);
    }

    public ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}