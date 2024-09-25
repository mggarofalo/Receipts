using FluentValidation.Results;
using MudBlazor;

namespace Client.Common;

public static class SnackbarMethods
{
	public static void ShowSuccessMessage(this ISnackbar snackbar, string message)
	{
		snackbar.Add(message, Severity.Success);
	}

	public static void ShowErrorMessage(this ISnackbar snackbar, string message)
	{
		snackbar.Add(message, Severity.Error);
	}

	public static void ShowValidationErrors(this ISnackbar snackbar, ValidationResult validationResult)
	{
		foreach (ValidationFailure error in validationResult.Errors)
		{
			snackbar.ShowErrorMessage(error.ErrorMessage);
		}
	}
}
