using FluentValidation;
using MediatR;

namespace Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
	: IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
{
	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		if (!validators.Any())
		{
			return await next(cancellationToken);
		}

		FluentValidation.Results.ValidationResult[] results = await Task.WhenAll(
			validators.Select(v => v.ValidateAsync(new ValidationContext<TRequest>(request), cancellationToken)));

		List<FluentValidation.Results.ValidationFailure> failures = results
			.SelectMany(r => r.Errors)
			.Where(f => f is not null)
			.ToList();

		if (failures.Count > 0)
		{
			throw new ValidationException(failures);
		}

		return await next(cancellationToken);
	}
}
