using System.Security.Claims;
using API.Generated.Dtos;
using Application.Interfaces.Services;
using Asp.Versioning;
using Common;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController(
	UserManager<ApplicationUser> userManager,
	ITokenService tokenService,
	IUserService userService,
	IAuthAuditService authAuditService,
	ILogger<AuthController> logger) : ControllerBase
{
	[HttpPost("login")]
	[AllowAnonymous]
	[EndpointSummary("Login with email and password")]
	[ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType<OAuthErrorResponse>(StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginRequest request)
	{
		ApplicationUser? user = await userManager.FindByEmailAsync(request.Email);
		if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
		{
			await LogAuthEventAsync(nameof(AuthEventType.LoginFailed), user?.Id, request.Email, false, "Invalid credentials");
			return Unauthorized(new OAuthErrorResponse
			{
				Error = OAuthErrorResponseError.Invalid_grant,
				Error_description = "Invalid email or password",
			});
		}

		if (await userManager.IsLockedOutAsync(user))
		{
			await LogAuthEventAsync(nameof(AuthEventType.LoginFailed), user.Id, request.Email, false, "Account disabled");
			return Unauthorized(new OAuthErrorResponse
			{
				Error = OAuthErrorResponseError.Invalid_grant,
				Error_description = "Account is disabled",
			});
		}

		IList<string> roles = await userManager.GetRolesAsync(user);
		string accessToken = tokenService.GenerateAccessToken(user.Id, user.Email!, roles, user.MustResetPassword);
		string refreshToken = tokenService.GenerateRefreshToken();

		user.RefreshToken = refreshToken;
		user.RefreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(30);
		user.LastLoginAt = DateTimeOffset.UtcNow;
		await userManager.UpdateAsync(user);

		await LogAuthEventAsync(nameof(AuthEventType.Login), user.Id, user.Email, true);

		return Ok(new TokenResponse
		{
			AccessToken = accessToken,
			RefreshToken = refreshToken,
			ExpiresIn = 3600,
			MustResetPassword = user.MustResetPassword,
			TokenType = "Bearer",
			Scope = string.Join(" ", roles),
		});
	}

	[HttpPost("refresh")]
	[AllowAnonymous]
	[EndpointSummary("Refresh access token")]
	[ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<TokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
	{
		string? userId = await userService.FindUserIdByRefreshTokenAsync(request.RefreshToken, cancellationToken);
		ApplicationUser? user = userId is not null ? await userManager.FindByIdAsync(userId) : null;

		if (user is null
			|| user.RefreshTokenExpiresAt is null
			|| user.RefreshTokenExpiresAt < DateTimeOffset.UtcNow)
		{
			return Unauthorized();
		}

		IList<string> roles = await userManager.GetRolesAsync(user);
		string accessToken = tokenService.GenerateAccessToken(user.Id, user.Email!, roles, user.MustResetPassword);
		string newRefreshToken = tokenService.GenerateRefreshToken();

		user.RefreshToken = newRefreshToken;
		user.RefreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(30);
		await userManager.UpdateAsync(user);

		return Ok(new TokenResponse
		{
			AccessToken = accessToken,
			RefreshToken = newRefreshToken,
			ExpiresIn = 3600,
			MustResetPassword = user.MustResetPassword,
			TokenType = "Bearer",
			Scope = string.Join(" ", roles),
		});
	}

	[HttpPost("logout")]
	[Authorize]
	[EndpointSummary("Logout and invalidate refresh token")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Logout()
	{
		string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (userId is null)
		{
			return Unauthorized();
		}

		ApplicationUser? user = await userManager.FindByIdAsync(userId);
		if (user is not null)
		{
			user.RefreshToken = null;
			user.RefreshTokenExpiresAt = null;
			await userManager.UpdateAsync(user);
		}

		await LogAuthEventAsync(nameof(AuthEventType.Logout), userId, user?.Email, true);

		return NoContent();
	}

	[HttpPost("change-password")]
	[Authorize]
	[EndpointSummary("Change password (required on first login)")]
	[ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType<OAuthErrorResponse>(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<TokenResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
	{
		string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (userId is null)
		{
			return Unauthorized();
		}

		ApplicationUser? user = await userManager.FindByIdAsync(userId);
		if (user is null)
		{
			return Unauthorized();
		}

		IdentityResult result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
		if (!result.Succeeded)
		{
			return BadRequest(new OAuthErrorResponse
			{
				Error = OAuthErrorResponseError.Invalid_request,
				Error_description = string.Join("; ", result.Errors.Select(e => e.Description)),
			});
		}

		user.MustResetPassword = false;

		IList<string> roles = await userManager.GetRolesAsync(user);
		string accessToken = tokenService.GenerateAccessToken(user.Id, user.Email!, roles, false);
		string refreshToken = tokenService.GenerateRefreshToken();

		user.RefreshToken = refreshToken;
		user.RefreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(30);
		await userManager.UpdateAsync(user);

		await LogAuthEventAsync(nameof(AuthEventType.PasswordChanged), user.Id, user.Email, true);

		return Ok(new TokenResponse
		{
			AccessToken = accessToken,
			RefreshToken = refreshToken,
			ExpiresIn = 3600,
			MustResetPassword = false,
			TokenType = "Bearer",
			Scope = string.Join(" ", roles),
		});
	}

	[HttpPost("introspect")]
	[Authorize]
	[EndpointSummary("Introspect a token per RFC 7662")]
	[ProducesResponseType<TokenIntrospectionResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<TokenIntrospectionResponse>> IntrospectToken(
		[FromBody] TokenIntrospectionRequest request,
		CancellationToken cancellationToken)
	{
		if (request.TokenTypeHint == TokenIntrospectionRequestTokenTypeHint.RefreshToken)
		{
			string? userId = await userService.FindUserIdByRefreshTokenAsync(request.Token, cancellationToken);
			ApplicationUser? user = userId is not null ? await userManager.FindByIdAsync(userId) : null;

			if (user is null
				|| user.RefreshTokenExpiresAt is null
				|| user.RefreshTokenExpiresAt < DateTimeOffset.UtcNow)
			{
				return Ok(new TokenIntrospectionResponse { Active = false });
			}

			IList<string> roles = await userManager.GetRolesAsync(user);

			return Ok(new TokenIntrospectionResponse
			{
				Active = true,
				Scope = string.Join(" ", roles),
				Username = user.Email ?? string.Empty,
				TokenType = "refresh_token",
				Exp = user.RefreshTokenExpiresAt.Value.ToUnixTimeSeconds(),
				Sub = user.Id,
			});
		}

		TokenIntrospectionResult introspection = tokenService.IntrospectAccessToken(request.Token);

		return Ok(new TokenIntrospectionResponse
		{
			Active = introspection.Active,
			Scope = introspection.Scope ?? string.Empty,
			Username = introspection.Username ?? string.Empty,
			TokenType = introspection.TokenType ?? string.Empty,
			Exp = introspection.Exp ?? 0,
			Iat = introspection.Iat ?? 0,
			Sub = introspection.Sub ?? string.Empty,
		});
	}

	[HttpPost("revoke")]
	[Authorize]
	[EndpointSummary("Revoke a token per RFC 7009")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<IActionResult> RevokeToken(
		[FromBody] TokenRevocationRequest request,
		CancellationToken cancellationToken)
	{
		// Per RFC 7009, always return 200 regardless of whether the token was found
		string? userId = await userService.FindUserIdByRefreshTokenAsync(request.Token, cancellationToken);
		if (userId is not null)
		{
			ApplicationUser? user = await userManager.FindByIdAsync(userId);
			if (user is not null)
			{
				user.RefreshToken = null;
				user.RefreshTokenExpiresAt = null;
				await userManager.UpdateAsync(user);

				await LogAuthEventAsync(nameof(AuthEventType.TokenRevoked), user.Id, user.Email, true);
			}
		}

		return Ok();
	}

	private async Task LogAuthEventAsync(string eventType, string? userId, string? username, bool success, string? failureReason = null)
	{
		try
		{
			await authAuditService.LogAsync(new AuthAuditEntryDto(
				Guid.NewGuid(),
				eventType,
				userId,
				null,
				username,
				success,
				failureReason,
				HttpContext.Connection.RemoteIpAddress?.ToString(),
				Request.Headers.UserAgent.ToString(),
				DateTimeOffset.UtcNow,
				null));
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to log auth audit event {EventType}", eventType);
		}
	}
}
