using Application.Interfaces.Services;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services;

public class TokenService(IConfiguration configuration) : ITokenService
{
	public string GenerateAccessToken(string userId, string email, IList<string> roles)
	{
		string key = configuration[ConfigurationVariables.JwtKey] ?? "build-time-placeholder-key-32-chars!!";
		string issuer = configuration[ConfigurationVariables.JwtIssuer] ?? "receipts-api";
		string audience = configuration[ConfigurationVariables.JwtAudience] ?? "receipts-app";

		SymmetricSecurityKey signingKey = new(Encoding.UTF8.GetBytes(key));
		SigningCredentials credentials = new(signingKey, SecurityAlgorithms.HmacSha256);

		SecurityTokenDescriptor descriptor = new()
		{
			Subject = new ClaimsIdentity(
			[
				new Claim(ClaimTypes.NameIdentifier, userId),
				new Claim(ClaimTypes.Email, email),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				.. roles.Select(r => new Claim(ClaimTypes.Role, r)),
			]),
			Expires = DateTime.UtcNow.AddHours(1),
			Issuer = issuer,
			Audience = audience,
			SigningCredentials = credentials,
		};

		JsonWebTokenHandler handler = new();
		return handler.CreateToken(descriptor);
	}

	public string GenerateRefreshToken()
	{
		byte[] bytes = new byte[64];
		RandomNumberGenerator.Fill(bytes);
		return Convert.ToBase64String(bytes);
	}
}
