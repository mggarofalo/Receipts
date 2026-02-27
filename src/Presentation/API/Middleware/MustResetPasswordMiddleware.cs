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
				context.Response.StatusCode = StatusCodes.Status403Forbidden;
				context.Response.ContentType = "application/json";
				await context.Response.WriteAsJsonAsync(new { error = "Password change required" });
				return;
			}
		}

		await next(context);
	}
}
