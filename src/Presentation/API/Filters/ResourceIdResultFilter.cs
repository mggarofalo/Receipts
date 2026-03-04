using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

public class ResourceIdResultFilter : IAsyncResultFilter
{
	public const string HeaderName = "X-Resource-Id";

	public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
	{
		if (!HttpMethods.IsDelete(context.HttpContext.Request.Method))
		{
			string? resourceId = TryGetIdFromResult(context.Result)
				?? TryGetIdFromRouteData(context.RouteData);

			if (resourceId is not null)
			{
				context.HttpContext.Response.Headers[HeaderName] = resourceId;
			}
		}

		await next();
	}

	private static string? TryGetIdFromResult(IActionResult result)
	{
		if (result is not ObjectResult objectResult || objectResult.Value is null)
		{
			return null;
		}

		PropertyInfo? idProperty = objectResult.Value.GetType().GetProperty("Id");
		if (idProperty is not null && idProperty.PropertyType == typeof(Guid))
		{
			Guid id = (Guid)idProperty.GetValue(objectResult.Value)!;
			return id.ToString();
		}

		return null;
	}

	private static string? TryGetIdFromRouteData(RouteData routeData)
	{
		if (routeData.Values.TryGetValue("id", out object? value) && value is not null)
		{
			string raw = value.ToString()!;
			return Guid.TryParse(raw, out Guid parsed) ? parsed.ToString() : null;
		}

		return null;
	}
}
