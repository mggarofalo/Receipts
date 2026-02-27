using API.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace Presentation.API.Tests.Services;

public class CurrentUserAccessorTests
{
	private static CurrentUserAccessor CreateAccessor(ClaimsPrincipal? principal = null, string? ipAddress = null, string? userAgent = null)
	{
		Mock<IHttpContextAccessor> mockAccessor = new();
		if (principal is not null)
		{
			DefaultHttpContext httpContext = new()
			{
				User = principal
			};
			if (ipAddress is not null)
			{
				httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(ipAddress);
			}
			if (userAgent is not null)
			{
				httpContext.Request.Headers.UserAgent = userAgent;
			}
			mockAccessor.Setup(x => x.HttpContext).Returns(httpContext);
		}
		return new CurrentUserAccessor(mockAccessor.Object);
	}

	[Fact]
	public void UserId_ReturnsNameIdentifierClaim()
	{
		// Arrange
		ClaimsPrincipal principal = new(new ClaimsIdentity(
		[
			new Claim(ClaimTypes.NameIdentifier, "user-123")
		]));
		CurrentUserAccessor accessor = CreateAccessor(principal);

		// Act & Assert
		Assert.Equal("user-123", accessor.UserId);
	}

	[Fact]
	public void UserId_ReturnsNullWhenNoClaim()
	{
		// Arrange
		ClaimsPrincipal principal = new(new ClaimsIdentity());
		CurrentUserAccessor accessor = CreateAccessor(principal);

		// Act & Assert
		Assert.Null(accessor.UserId);
	}

	[Fact]
	public void UserId_ReturnsNullWhenNoHttpContext()
	{
		// Arrange
		CurrentUserAccessor accessor = CreateAccessor();

		// Act & Assert
		Assert.Null(accessor.UserId);
	}

	[Fact]
	public void ApiKeyId_ReturnsGuidWhenValid()
	{
		// Arrange
		Guid expected = Guid.NewGuid();
		ClaimsPrincipal principal = new(new ClaimsIdentity(
		[
			new Claim("ApiKeyId", expected.ToString())
		]));
		CurrentUserAccessor accessor = CreateAccessor(principal);

		// Act & Assert
		Assert.Equal(expected, accessor.ApiKeyId);
	}

	[Fact]
	public void ApiKeyId_ReturnsNullWhenInvalidGuid()
	{
		// Arrange
		ClaimsPrincipal principal = new(new ClaimsIdentity(
		[
			new Claim("ApiKeyId", "not-a-guid")
		]));
		CurrentUserAccessor accessor = CreateAccessor(principal);

		// Act & Assert
		Assert.Null(accessor.ApiKeyId);
	}

	[Fact]
	public void ApiKeyId_ReturnsNullWhenNoClaim()
	{
		// Arrange
		ClaimsPrincipal principal = new(new ClaimsIdentity());
		CurrentUserAccessor accessor = CreateAccessor(principal);

		// Act & Assert
		Assert.Null(accessor.ApiKeyId);
	}

	[Fact]
	public void IpAddress_ReturnsRemoteIpAddress()
	{
		// Arrange
		ClaimsPrincipal principal = new(new ClaimsIdentity());
		CurrentUserAccessor accessor = CreateAccessor(principal, ipAddress: "192.168.1.1");

		// Act & Assert
		Assert.Equal("192.168.1.1", accessor.IpAddress);
	}

	[Fact]
	public void UserAgent_ReturnsHeaderValue()
	{
		// Arrange
		ClaimsPrincipal principal = new(new ClaimsIdentity());
		CurrentUserAccessor accessor = CreateAccessor(principal, userAgent: "TestBrowser/1.0");

		// Act & Assert
		Assert.Equal("TestBrowser/1.0", accessor.UserAgent);
	}
}
