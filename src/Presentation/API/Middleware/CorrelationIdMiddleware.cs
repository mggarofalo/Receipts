using System.Diagnostics;
using Serilog.Context;

namespace API.Middleware;

public class CorrelationIdMiddleware(RequestDelegate next)
{
	public const string HeaderName = "X-Correlation-ID";
	public const string ItemKey = "CorrelationId";

	public async Task InvokeAsync(HttpContext context)
	{
		string correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
			?? Activity.Current?.TraceId.ToString()
			?? Guid.NewGuid().ToString();

		context.Items[ItemKey] = correlationId;

		context.Response.OnStarting(() =>
		{
			context.Response.Headers[HeaderName] = correlationId;
			return Task.CompletedTask;
		});

		using (LogContext.PushProperty(ItemKey, correlationId))
		{
			await next(context);
		}
	}
}
