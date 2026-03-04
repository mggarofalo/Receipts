using System.Text.Json;
using API.Middleware;
using Application.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Presentation.API.Tests.Middleware;

public class GlobalExceptionHandlerMiddlewareTests
{
	private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _loggerMock = new();

	[Fact]
	public async Task InvokeAsync_NoException_PassesThrough()
	{
		// Arrange
		DefaultHttpContext context = new();
		context.Response.Body = new MemoryStream();
		GlobalExceptionHandlerMiddleware middleware = new(_ => Task.CompletedTask, _loggerMock.Object);

		// Act
		await middleware.InvokeAsync(context);

		// Assert
		context.Response.StatusCode.Should().Be(200);
	}

	[Fact]
	public async Task InvokeAsync_UnhandledException_Returns500ProblemDetails()
	{
		// Arrange
		DefaultHttpContext context = new();
		context.Response.Body = new MemoryStream();
		context.Items[CorrelationIdMiddleware.ItemKey] = "test-correlation-id";

		GlobalExceptionHandlerMiddleware middleware = new(
			_ => throw new InvalidOperationException("Something broke"),
			_loggerMock.Object);

		// Act
		await middleware.InvokeAsync(context);

		// Assert
		context.Response.StatusCode.Should().Be(500);
		context.Response.ContentType.Should().Be("application/problem+json");

		ProblemDetails? problemDetails = await DeserializeProblemDetails(context.Response);
		problemDetails.Should().NotBeNull();
		problemDetails!.Status.Should().Be(500);
		problemDetails.Title.Should().Be("An error occurred while processing your request.");
		problemDetails.Extensions.Should().ContainKey("errorId");
		problemDetails.Extensions.Should().ContainKey("correlationId");
		problemDetails.Extensions["correlationId"]!.ToString().Should().Be("test-correlation-id");
	}

	[Fact]
	public async Task InvokeAsync_UnhandledException_DoesNotLeakExceptionMessage()
	{
		// Arrange
		string sensitiveMessage = "Connection string: Server=prod;Password=secret";
		DefaultHttpContext context = new();
		context.Response.Body = new MemoryStream();

		GlobalExceptionHandlerMiddleware middleware = new(
			_ => throw new Exception(sensitiveMessage),
			_loggerMock.Object);

		// Act
		await middleware.InvokeAsync(context);

		// Assert
		context.Response.Body.Position = 0;
		using StreamReader reader = new(context.Response.Body);
		string body = await reader.ReadToEndAsync();
		body.Should().NotContain(sensitiveMessage);
	}

	[Fact]
	public async Task InvokeAsync_DuplicateEntityException_Returns409ProblemDetails()
	{
		// Arrange
		DefaultHttpContext context = new();
		context.Response.Body = new MemoryStream();

		GlobalExceptionHandlerMiddleware middleware = new(
			_ => throw new DuplicateEntityException("Category 'Food' already exists"),
			_loggerMock.Object);

		// Act
		await middleware.InvokeAsync(context);

		// Assert
		context.Response.StatusCode.Should().Be(409);
		context.Response.ContentType.Should().Be("application/problem+json");

		ProblemDetails? problemDetails = await DeserializeProblemDetails(context.Response);
		problemDetails.Should().NotBeNull();
		problemDetails!.Status.Should().Be(409);
		problemDetails.Title.Should().Be("Conflict");
		problemDetails.Detail.Should().Be("Category 'Food' already exists");
	}

	[Fact]
	public async Task InvokeAsync_KeyNotFoundException_Returns404ProblemDetails()
	{
		// Arrange
		DefaultHttpContext context = new();
		context.Response.Body = new MemoryStream();

		GlobalExceptionHandlerMiddleware middleware = new(
			_ => throw new KeyNotFoundException("API key not found"),
			_loggerMock.Object);

		// Act
		await middleware.InvokeAsync(context);

		// Assert
		context.Response.StatusCode.Should().Be(404);
		context.Response.ContentType.Should().Be("application/problem+json");

		ProblemDetails? problemDetails = await DeserializeProblemDetails(context.Response);
		problemDetails.Should().NotBeNull();
		problemDetails!.Status.Should().Be(404);
		problemDetails.Title.Should().Be("Not Found");
		problemDetails.Detail.Should().Be("API key not found");
	}

	[Fact]
	public async Task InvokeAsync_UnhandledException_LogsError()
	{
		// Arrange
		DefaultHttpContext context = new();
		context.Response.Body = new MemoryStream();

		GlobalExceptionHandlerMiddleware middleware = new(
			_ => throw new InvalidOperationException("test error"),
			_loggerMock.Object);

		// Act
		await middleware.InvokeAsync(context);

		// Assert
		_loggerMock.Verify(
			x => x.Log(
				LogLevel.Error,
				It.IsAny<EventId>(),
				It.IsAny<It.IsAnyType>(),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);
	}

	private static async Task<ProblemDetails?> DeserializeProblemDetails(HttpResponse response)
	{
		response.Body.Position = 0;
		return await JsonSerializer.DeserializeAsync<ProblemDetails>(
			response.Body,
			new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
	}
}
