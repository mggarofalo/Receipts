using FluentValidation;
using Mediator;

namespace Application.Behaviors;

public class ValidationBehavior<TMessage, TResponse>(IEnumerable<IValidator<TMessage>> validators)
	: IPipelineBehavior<TMessage, TResponse>
	where TMessage : notnull, IMessage
{
	public async ValueTask<TResponse> Handle(
		TMessage message,
		MessageHandlerDelegate<TMessage, TResponse> next,
		CancellationToken cancellationToken)
	{
		if (!validators.Any())
		{
			return await next(message, cancellationToken);
		}

		FluentValidation.Results.ValidationResult[] results = await Task.WhenAll(
			validators.Select(v => v.ValidateAsync(new ValidationContext<TMessage>(message), cancellationToken)));

		List<FluentValidation.Results.ValidationFailure> failures = results
			.SelectMany(r => r.Errors)
			.Where(f => f is not null)
			.ToList();

		if (failures.Count > 0)
		{
			throw new ValidationException(failures);
		}

		return await next(message, cancellationToken);
	}
}
