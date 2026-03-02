using API.Middleware;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Presentation.API.Tests.Middleware;

public class ValidationExceptionMiddlewareTests
{
	[Fact]
	public async Task InvokeAsync_NoException_PassesThrough()
	{
		// Arrange
		DefaultHttpContext context = new();
		context.Response.Body = new MemoryStream();
		ValidationExceptionMiddleware middleware = new(_ => Task.CompletedTask);

		// Act
		await middleware.InvokeAsync(context);

		// Assert
		context.Response.StatusCode.Should().Be(200);
	}

	[Fact]
	public async Task InvokeAsync_ValidationException_Returns400ProblemDetails()
	{
		// Arrange
		DefaultHttpContext context = new();
		context.Response.Body = new MemoryStream();

		List<ValidationFailure> failures =
		[
			new("Name", "Name is required"),
			new("Name", "Name must be at least 3 characters"),
			new("Email", "Email is invalid")
		];

		ValidationExceptionMiddleware middleware = new(_ =>
			throw new ValidationException(failures));

		// Act
		await middleware.InvokeAsync(context);

		// Assert
		context.Response.StatusCode.Should().Be(400);
		context.Response.ContentType.Should().Be("application/problem+json");

		context.Response.Body.Position = 0;
		using StreamReader reader = new(context.Response.Body);
		string body = await reader.ReadToEndAsync();

		ValidationProblemDetails? problemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(
			body,
			new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		problemDetails.Should().NotBeNull();
		problemDetails!.Status.Should().Be(400);
		problemDetails.Title.Should().Be("One or more validation errors occurred.");
		problemDetails.Errors.Should().ContainKey("Name");
		problemDetails.Errors["Name"].Should().HaveCount(2);
		problemDetails.Errors.Should().ContainKey("Email");
		problemDetails.Errors["Email"].Should().HaveCount(1);
	}

	[Fact]
	public async Task InvokeAsync_OtherException_Rethrows()
	{
		// Arrange
		DefaultHttpContext context = new();
		context.Response.Body = new MemoryStream();

		ValidationExceptionMiddleware middleware = new(_ =>
			throw new InvalidOperationException("Something else"));

		// Act
		Func<Task> act = () => middleware.InvokeAsync(context);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>();
	}
}
