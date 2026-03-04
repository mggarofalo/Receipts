using System.Security.Claims;
using API.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Presentation.API.Tests.Middleware;

public class AuthorizationLoggingHandlerTests
{
	private readonly Mock<ILogger<AuthorizationLoggingHandler>> _loggerMock = new();

	[Fact]
	public async Task HandleAsync_Forbidden_LogsWarning()
	{
		// Arrange
		AuthorizationLoggingHandler handler = new(_loggerMock.Object);

		Mock<IAuthenticationService> authServiceMock = new();
		authServiceMock
			.Setup(a => a.ForbidAsync(It.IsAny<HttpContext>(), It.IsAny<string?>(), It.IsAny<AuthenticationProperties?>()))
			.Returns(Task.CompletedTask);

		ServiceCollection services = new();
		services.AddSingleton(authServiceMock.Object);
		ServiceProvider serviceProvider = services.BuildServiceProvider();

		DefaultHttpContext context = new()
		{
			RequestServices = serviceProvider,
			User = new ClaimsPrincipal(new ClaimsIdentity(
			[
				new Claim(ClaimTypes.NameIdentifier, "user-123")
			]))
		};
		context.Request.Method = "GET";
		context.Request.Path = "/api/admin";

		AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
			.RequireAuthenticatedUser()
			.Build();

		AuthorizationFailure failure = AuthorizationFailure.ExplicitFail();
		PolicyAuthorizationResult authorizeResult = PolicyAuthorizationResult.Forbid(failure);

		RequestDelegate next = _ => Task.CompletedTask;

		// Act
		await handler.HandleAsync(next, context, policy, authorizeResult);

		// Assert
		_loggerMock.Verify(
			x => x.Log(
				LogLevel.Warning,
				It.IsAny<EventId>(),
				It.IsAny<It.IsAnyType>(),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);
	}

	[Fact]
	public async Task HandleAsync_Success_DoesNotLogWarning()
	{
		// Arrange
		AuthorizationLoggingHandler handler = new(_loggerMock.Object);
		DefaultHttpContext context = new();

		AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
			.RequireAuthenticatedUser()
			.Build();

		PolicyAuthorizationResult authorizeResult = PolicyAuthorizationResult.Success();

		bool nextCalled = false;
		RequestDelegate next = _ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		};

		// Act
		await handler.HandleAsync(next, context, policy, authorizeResult);

		// Assert
		_loggerMock.Verify(
			x => x.Log(
				LogLevel.Warning,
				It.IsAny<EventId>(),
				It.IsAny<It.IsAnyType>(),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Never);
		nextCalled.Should().BeTrue();
	}
}
