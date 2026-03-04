using Application.Interfaces.Services;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Tests.Services;

public class TokenServiceTests
{
	private readonly TokenService _tokenService;

	public TokenServiceTests()
	{
		Dictionary<string, string?> config = new()
		{
			["Jwt:Key"] = "test-signing-key-that-is-at-least-32-chars!!",
			["Jwt:Issuer"] = "test-issuer",
			["Jwt:Audience"] = "test-audience",
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(config)
			.Build();

		_tokenService = new TokenService(configuration);
	}

	[Fact]
	public void IntrospectAccessToken_ValidToken_ReturnsActive()
	{
		// Arrange
		string token = _tokenService.GenerateAccessToken("user-123", "test@example.com", new List<string> { "Admin" }, false);

		// Act
		TokenIntrospectionResult result = _tokenService.IntrospectAccessToken(token);

		// Assert
		result.Active.Should().BeTrue();
		result.Sub.Should().Be("user-123");
		result.Username.Should().Be("test@example.com");
		result.TokenType.Should().Be("Bearer");
		result.Scope.Should().Be("Admin");
		result.Exp.Should().BeGreaterThan(0);
		result.Iat.Should().NotBeNull();
	}

	[Fact]
	public void IntrospectAccessToken_MalformedToken_ReturnsInactive()
	{
		// Act
		TokenIntrospectionResult result = _tokenService.IntrospectAccessToken("not-a-valid-jwt");

		// Assert
		result.Active.Should().BeFalse();
		result.Sub.Should().BeNull();
		result.Username.Should().BeNull();
		result.TokenType.Should().BeNull();
		result.Scope.Should().BeNull();
	}

	[Fact]
	public void IntrospectAccessToken_TokenWithWrongKey_ReturnsInactive()
	{
		// Arrange — generate token with different key
		Dictionary<string, string?> otherConfig = new()
		{
			["Jwt:Key"] = "different-signing-key-also-32-chars!!",
			["Jwt:Issuer"] = "test-issuer",
			["Jwt:Audience"] = "test-audience",
		};
		IConfiguration otherConfiguration = new ConfigurationBuilder()
			.AddInMemoryCollection(otherConfig)
			.Build();
		TokenService otherService = new(otherConfiguration);
		string token = otherService.GenerateAccessToken("user-123", "test@example.com", new List<string> { "Admin" }, false);

		// Act
		TokenIntrospectionResult result = _tokenService.IntrospectAccessToken(token);

		// Assert
		result.Active.Should().BeFalse();
	}

	[Fact]
	public void IntrospectAccessToken_MultipleRoles_ReturnsSpaceSeparatedScope()
	{
		// Arrange
		string token = _tokenService.GenerateAccessToken("user-123", "test@example.com", new List<string> { "Admin", "User" }, false);

		// Act
		TokenIntrospectionResult result = _tokenService.IntrospectAccessToken(token);

		// Assert
		result.Active.Should().BeTrue();
		result.Scope.Should().Be("Admin User");
	}
}
