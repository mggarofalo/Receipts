using System.Security.Claims;
using System.Text.Json;
using API.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.API.Tests.Middleware;

public class MustResetPasswordMiddlewareTests
{
	[Fact]
	public async Task InvokeAsync_PassesThrough_WhenUnauthenticated()
	{
		// Arrange
		DefaultHttpContext context = new();
		bool nextCalled = false;

		MustResetPasswordMiddleware middleware = new(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		});

		// Act
		await middleware.InvokeAsync(context);

		// Assert
		nextCalled.Should().BeTrue();
	}

	[Fact]
	public async Task InvokeAsync_PassesThrough_WhenAuthenticatedWithoutMustResetClaim()
	{
		// Arrange
		DefaultHttpContext context = new();
		context.User = new ClaimsPrincipal(new ClaimsIdentity(
		[
			new Claim(ClaimTypes.NameIdentifier, "user-1"),
		], "TestScheme"));
		bool nextCalled = false;

		MustResetPasswordMiddleware middleware = new(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		});

		// Act
		await middleware.InvokeAsync(context);

		// Assert
		nextCalled.Should().BeTrue();
	}

	[Fact]
	public async Task InvokeAsync_PassesThrough_WhenMustResetAndPathIsChangePassword()
	{
		// Arrange
		DefaultHttpContext context = CreateMustResetContext("/api/auth/change-password");
		bool nextCalled = false;

		MustResetPasswordMiddleware middleware = new(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		});

		// Act
		await middleware.InvokeAsync(context);

		// Assert
		nextCalled.Should().BeTrue();
	}

	[Fact]
	public async Task InvokeAsync_PassesThrough_WhenMustResetAndPathIsLogout()
	{
		// Arrange
		DefaultHttpContext context = CreateMustResetContext("/api/auth/logout");
		bool nextCalled = false;

		MustResetPasswordMiddleware middleware = new(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		});

		// Act
		await middleware.InvokeAsync(context);

		// Assert
		nextCalled.Should().BeTrue();
	}

	[Fact]
	public async Task InvokeAsync_Returns403WithProblemDetails_WhenMustResetAndPathIsOther()
	{
		// Arrange
		DefaultHttpContext context = CreateMustResetContext("/api/receipts");
		context.Response.Body = new MemoryStream();

		MustResetPasswordMiddleware middleware = new(_ => Task.CompletedTask);

		// Act
		await middleware.InvokeAsync(context);

		// Assert
		context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
		context.Response.ContentType.Should().Contain("application/problem+json");

		context.Response.Body.Position = 0;
		ProblemDetails? problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(
			context.Response.Body,
			new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		problemDetails.Should().NotBeNull();
		problemDetails!.Status.Should().Be(403);
		problemDetails.Title.Should().Be("Forbidden");
		problemDetails.Detail.Should().Be("Password change required");
	}

	[Fact]
	public async Task InvokeAsync_DoesNotCallNext_WhenMustResetAndPathIsBlocked()
	{
		// Arrange
		DefaultHttpContext context = CreateMustResetContext("/api/dashboard/summary");
		context.Response.Body = new MemoryStream();
		bool nextCalled = false;

		MustResetPasswordMiddleware middleware = new(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		});

		// Act
		await middleware.InvokeAsync(context);

		// Assert
		nextCalled.Should().BeFalse();
	}

	private static DefaultHttpContext CreateMustResetContext(string path)
	{
		DefaultHttpContext context = new();
		context.User = new ClaimsPrincipal(new ClaimsIdentity(
		[
			new Claim(ClaimTypes.NameIdentifier, "user-1"),
			new Claim("must_reset_password", "true"),
		], "TestScheme"));
		context.Request.Path = path;
		return context;
	}
}
