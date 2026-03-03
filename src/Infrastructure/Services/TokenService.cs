using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Interfaces.Services;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public class TokenService(IConfiguration configuration) : ITokenService
{
	public string GenerateAccessToken(string userId, string email, IList<string> roles, bool mustResetPassword)
	{
		string key = configuration[ConfigurationVariables.JwtKey] ?? "build-time-placeholder-key-32-chars!!";
		string issuer = configuration[ConfigurationVariables.JwtIssuer] ?? "receipts-api";
		string audience = configuration[ConfigurationVariables.JwtAudience] ?? "receipts-app";

		SymmetricSecurityKey signingKey = new(Encoding.UTF8.GetBytes(key));
		SigningCredentials credentials = new(signingKey, SecurityAlgorithms.HmacSha256);

		List<Claim> claims =
		[
			new Claim(ClaimTypes.NameIdentifier, userId),
			new Claim(ClaimTypes.Email, email),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			.. roles.Select(r => new Claim(ClaimTypes.Role, r)),
		];

		if (mustResetPassword)
		{
			claims.Add(new Claim("must_reset_password", "true"));
		}

		SecurityTokenDescriptor descriptor = new()
		{
			Subject = new ClaimsIdentity(claims),
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
