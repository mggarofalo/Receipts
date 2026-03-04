using System.Security.Claims;
using API.Generated.Dtos;
using Application.Interfaces.Services;
using Asp.Versioning;
using Common;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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
	public async Task<Results<Ok<TokenResponse>, UnauthorizedHttpResult>> Login([FromBody] LoginRequest request)
	{
		ApplicationUser? user = await userManager.FindByEmailAsync(request.Email);
		if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
		{
			await LogAuthEventAsync(nameof(AuthEventType.LoginFailed), user?.Id, request.Email, false, "Invalid credentials");
			return TypedResults.Unauthorized();
		}

		if (await userManager.IsLockedOutAsync(user))
		{
			await LogAuthEventAsync(nameof(AuthEventType.LoginFailed), user.Id, request.Email, false, "Account disabled");
			return TypedResults.Unauthorized();
		}

		IList<string> roles = await userManager.GetRolesAsync(user);
		string accessToken = tokenService.GenerateAccessToken(user.Id, user.Email!, roles, user.MustResetPassword);
		string refreshToken = tokenService.GenerateRefreshToken();

		user.RefreshToken = refreshToken;
		user.RefreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(30);
		user.LastLoginAt = DateTimeOffset.UtcNow;
		await userManager.UpdateAsync(user);

		await LogAuthEventAsync(nameof(AuthEventType.Login), user.Id, user.Email, true);

		return TypedResults.Ok(new TokenResponse
		{
			AccessToken = accessToken,
			RefreshToken = refreshToken,
			ExpiresIn = 3600,
			MustResetPassword = user.MustResetPassword,
		});
	}

	[HttpPost("refresh")]
	[AllowAnonymous]
	[EndpointSummary("Refresh access token")]
	public async Task<Results<Ok<TokenResponse>, UnauthorizedHttpResult>> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
	{
		string? userId = await userService.FindUserIdByRefreshTokenAsync(request.RefreshToken, cancellationToken);
		ApplicationUser? user = userId is not null ? await userManager.FindByIdAsync(userId) : null;

		if (user is null
			|| user.RefreshTokenExpiresAt is null
			|| user.RefreshTokenExpiresAt < DateTimeOffset.UtcNow)
		{
			return TypedResults.Unauthorized();
		}

		IList<string> roles = await userManager.GetRolesAsync(user);
		string accessToken = tokenService.GenerateAccessToken(user.Id, user.Email!, roles, user.MustResetPassword);
		string newRefreshToken = tokenService.GenerateRefreshToken();

		user.RefreshToken = newRefreshToken;
		user.RefreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(30);
		await userManager.UpdateAsync(user);

		return TypedResults.Ok(new TokenResponse
		{
			AccessToken = accessToken,
			RefreshToken = newRefreshToken,
			ExpiresIn = 3600,
			MustResetPassword = user.MustResetPassword,
		});
	}

	[HttpPost("logout")]
	[Authorize]
	[EndpointSummary("Logout and invalidate refresh token")]
	public async Task<Results<NoContent, UnauthorizedHttpResult>> Logout()
	{
		string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (userId is null)
		{
			return TypedResults.Unauthorized();
		}

		ApplicationUser? user = await userManager.FindByIdAsync(userId);
		if (user is not null)
		{
			user.RefreshToken = null;
			user.RefreshTokenExpiresAt = null;
			await userManager.UpdateAsync(user);
		}

		await LogAuthEventAsync(nameof(AuthEventType.Logout), userId, user?.Email, true);

		return TypedResults.NoContent();
	}

	[HttpPost("change-password")]
	[Authorize]
	[EndpointSummary("Change password (required on first login)")]
	public async Task<Results<Ok<TokenResponse>, BadRequest<IEnumerable<string>>, UnauthorizedHttpResult>> ChangePassword([FromBody] ChangePasswordRequest request)
	{
		string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (userId is null)
		{
			return TypedResults.Unauthorized();
		}

		ApplicationUser? user = await userManager.FindByIdAsync(userId);
		if (user is null)
		{
			return TypedResults.Unauthorized();
		}

		IdentityResult result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
		if (!result.Succeeded)
		{
			return TypedResults.BadRequest(result.Errors.Select(e => e.Description));
		}

		user.MustResetPassword = false;

		IList<string> roles = await userManager.GetRolesAsync(user);
		string accessToken = tokenService.GenerateAccessToken(user.Id, user.Email!, roles, false);
		string refreshToken = tokenService.GenerateRefreshToken();

		user.RefreshToken = refreshToken;
		user.RefreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(30);
		await userManager.UpdateAsync(user);

		await LogAuthEventAsync(nameof(AuthEventType.PasswordChanged), user.Id, user.Email, true);

		return TypedResults.Ok(new TokenResponse
		{
			AccessToken = accessToken,
			RefreshToken = refreshToken,
			ExpiresIn = 3600,
			MustResetPassword = false,
		});
	}

	private async Task LogAuthEventAsync(string eventType, string? userId, string? username, bool success, string? failureReason = null)
	{
		try
		{
			await authAuditService.LogAsync(new Application.Interfaces.Services.AuthAuditEntryDto(
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
