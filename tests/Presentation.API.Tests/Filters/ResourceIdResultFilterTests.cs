using API.Filters;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace Presentation.API.Tests.Filters;

public class ResourceIdResultFilterTests
{
	private record TestResource(Guid Id, string Name);

	private record TestResourceWithoutId(string Name);

	private static ResultExecutingContext CreateContext(
		IActionResult result,
		string method = "GET",
		RouteData? routeData = null)
	{
		DefaultHttpContext httpContext = new();
		httpContext.Request.Method = method;
		ActionContext actionContext = new(
			httpContext,
			routeData ?? new RouteData(),
			new ActionDescriptor());
		return new ResultExecutingContext(
			actionContext,
			[],
			result,
			new object());
	}

	private static ResultExecutedContext CreateExecutedContext(ResultExecutingContext executingContext)
	{
		return new ResultExecutedContext(
			executingContext,
			[],
			executingContext.Result,
			executingContext.Controller);
	}

	[Fact]
	public async Task OnResultExecutionAsync_ObjectResultWithIdProperty_SetsHeader()
	{
		// Arrange
		Guid expected = Guid.NewGuid();
		ResourceIdResultFilter filter = new();
		ObjectResult objectResult = new(new TestResource(expected, "Test"));
		ResultExecutingContext context = CreateContext(objectResult);

		// Act
		await filter.OnResultExecutionAsync(context, () =>
			Task.FromResult(CreateExecutedContext(context)));

		// Assert
		context.HttpContext.Response.Headers[ResourceIdResultFilter.HeaderName]
			.ToString()
			.Should().Be(expected.ToString());
	}

	[Fact]
	public async Task OnResultExecutionAsync_ObjectResultWithoutIdProperty_FallsBackToRouteData()
	{
		// Arrange
		Guid expected = Guid.NewGuid();
		ResourceIdResultFilter filter = new();
		ObjectResult objectResult = new(new TestResourceWithoutId("Test"));
		RouteData routeData = new();
		routeData.Values["id"] = expected.ToString();
		ResultExecutingContext context = CreateContext(objectResult, routeData: routeData);

		// Act
		await filter.OnResultExecutionAsync(context, () =>
			Task.FromResult(CreateExecutedContext(context)));

		// Assert
		context.HttpContext.Response.Headers[ResourceIdResultFilter.HeaderName]
			.ToString()
			.Should().Be(expected.ToString());
	}

	[Fact]
	public async Task OnResultExecutionAsync_DeleteRequest_SkipsHeader()
	{
		// Arrange
		Guid resourceId = Guid.NewGuid();
		ResourceIdResultFilter filter = new();
		ObjectResult objectResult = new(new TestResource(resourceId, "Test"));
		ResultExecutingContext context = CreateContext(objectResult, method: "DELETE");

		// Act
		await filter.OnResultExecutionAsync(context, () =>
			Task.FromResult(CreateExecutedContext(context)));

		// Assert
		context.HttpContext.Response.Headers.ContainsKey(ResourceIdResultFilter.HeaderName)
			.Should().BeFalse();
	}

	[Fact]
	public async Task OnResultExecutionAsync_NoIdExtractable_SkipsHeader()
	{
		// Arrange
		ResourceIdResultFilter filter = new();
		ObjectResult objectResult = new(new TestResourceWithoutId("Test"));
		ResultExecutingContext context = CreateContext(objectResult);

		// Act
		await filter.OnResultExecutionAsync(context, () =>
			Task.FromResult(CreateExecutedContext(context)));

		// Assert
		context.HttpContext.Response.Headers.ContainsKey(ResourceIdResultFilter.HeaderName)
			.Should().BeFalse();
	}

	[Fact]
	public async Task OnResultExecutionAsync_NonObjectResult_SkipsHeader()
	{
		// Arrange
		ResourceIdResultFilter filter = new();
		NoContentResult noContentResult = new();
		ResultExecutingContext context = CreateContext(noContentResult);

		// Act
		await filter.OnResultExecutionAsync(context, () =>
			Task.FromResult(CreateExecutedContext(context)));

		// Assert
		context.HttpContext.Response.Headers.ContainsKey(ResourceIdResultFilter.HeaderName)
			.Should().BeFalse();
	}

	[Fact]
	public async Task OnResultExecutionAsync_RouteDataIdWithNoObjectResult_SetsHeaderFromRouteData()
	{
		// Arrange
		Guid expected = Guid.NewGuid();
		ResourceIdResultFilter filter = new();
		NoContentResult noContentResult = new();
		RouteData routeData = new();
		routeData.Values["id"] = expected.ToString();
		ResultExecutingContext context = CreateContext(noContentResult, routeData: routeData);

		// Act
		await filter.OnResultExecutionAsync(context, () =>
			Task.FromResult(CreateExecutedContext(context)));

		// Assert
		context.HttpContext.Response.Headers[ResourceIdResultFilter.HeaderName]
			.ToString()
			.Should().Be(expected.ToString());
	}
}
