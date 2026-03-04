using Microsoft.AspNetCore.Mvc;

namespace API.Middleware;

public class MustResetPasswordMiddleware(RequestDelegate next)
{
	public async Task InvokeAsync(HttpContext context)
	{
		if (context.User.Identity?.IsAuthenticated == true
			&& context.User.FindFirst("must_reset_password")?.Value == "true")
		{
			string path = context.Request.Path.Value ?? "";
			if (!path.Equals("/api/auth/change-password", StringComparison.OrdinalIgnoreCase)
				&& !path.Equals("/api/auth/logout", StringComparison.OrdinalIgnoreCase))
			{
				ProblemDetails problemDetails = new()
				{
					Status = StatusCodes.Status403Forbidden,
					Title = "Forbidden",
					Detail = "Password change required",
					Type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
				};

				context.Response.StatusCode = StatusCodes.Status403Forbidden;
				await context.Response.WriteAsJsonAsync(problemDetails, options: null, contentType: "application/problem+json");
				return;
			}
		}

		await next(context);
	}
}
