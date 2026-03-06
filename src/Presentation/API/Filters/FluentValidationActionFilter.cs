using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

public class FluentValidationActionFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		foreach (object? argument in context.ActionArguments.Values)
		{
			if (argument is null)
			{
				continue;
			}

			if (argument is System.Collections.IList { Count: 0 })
			{
				throw new ValidationException([
					new ValidationFailure("", "Request body must contain at least one item.")
				]);
			}

			Type validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());

			if (serviceProvider.GetService(validatorType) is not IValidator validator)
			{
				continue;
			}

			IValidationContext validationContext = new ValidationContext<object>(argument);
			ValidationResult result = await validator.ValidateAsync(validationContext);

			if (!result.IsValid)
			{
				throw new ValidationException(result.Errors);
			}
		}

		await next();
	}
}
