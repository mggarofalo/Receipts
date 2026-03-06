using Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace API.Middleware;

public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
{
	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await next(context);
		}
		catch (DuplicateEntityException ex)
		{
			logger.LogWarning(ex, "Duplicate entity: {Message}", ex.Message);

			ProblemDetails problemDetails = new()
			{
				Status = StatusCodes.Status409Conflict,
				Title = "Conflict",
				Detail = ex.Message,
				Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
			};

			context.Response.StatusCode = StatusCodes.Status409Conflict;
			await context.Response.WriteAsJsonAsync(problemDetails, options: null, contentType: "application/problem+json");
		}
		catch (KeyNotFoundException ex)
		{
			logger.LogWarning(ex, "Resource not found: {Message}", ex.Message);

			ProblemDetails problemDetails = new()
			{
				Status = StatusCodes.Status404NotFound,
				Title = "Not Found",
				Detail = ex.Message,
				Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
			};

			context.Response.StatusCode = StatusCodes.Status404NotFound;
			await context.Response.WriteAsJsonAsync(problemDetails, options: null, contentType: "application/problem+json");
		}
		catch (ArgumentException ex)
		{
			logger.LogWarning(ex, "Validation error: {Message}", ex.Message);

			ValidationProblemDetails problemDetails = new()
			{
				Status = StatusCodes.Status400BadRequest,
				Title = "Validation Error",
				Detail = ex.Message,
				Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
			};

			context.Response.StatusCode = StatusCodes.Status400BadRequest;
			await context.Response.WriteAsJsonAsync(problemDetails, options: null, contentType: "application/problem+json");
		}
		catch (Exception ex)
		{
			string errorId = Guid.NewGuid().ToString();
			string correlationId = context.Items[CorrelationIdMiddleware.ItemKey]?.ToString() ?? "";

			logger.LogError(ex,
				"Unhandled exception. ErrorId: {ErrorId}, CorrelationId: {CorrelationId}, Method: {Method}, Path: {Path}",
				errorId, correlationId, context.Request.Method, context.Request.Path);

			ProblemDetails problemDetails = new()
			{
				Status = StatusCodes.Status500InternalServerError,
				Title = "An error occurred while processing your request.",
				Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
				Extensions =
				{
					["errorId"] = errorId,
					["correlationId"] = correlationId,
				},
			};

			context.Response.StatusCode = StatusCodes.Status500InternalServerError;
			await context.Response.WriteAsJsonAsync(problemDetails, options: null, contentType: "application/problem+json");
		}
	}
}
