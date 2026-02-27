using API.Generated.Dtos;
using Application.Interfaces.Services;
using Common;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

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
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginRequest request)
	{
		try
		{
			ApplicationUser? user = await userManager.FindByEmailAsync(request.Email);
			if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
			{
				await LogAuthEventAsync(nameof(AuthEventType.LoginFailed), user?.Id, request.Email, false, "Invalid credentials");
				return Unauthorized();
			}

			if (await userManager.IsLockedOutAsync(user))
			{
				await LogAuthEventAsync(nameof(AuthEventType.LoginFailed), user.Id, request.Email, false, "Account disabled");
				return Unauthorized();
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
			});
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in {Method}", nameof(Login));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost("refresh")]
	[AllowAnonymous]
	[EndpointSummary("Refresh access token")]
	[ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<TokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
	{
		try
		{
			string? userId = await userService.FindUserIdByRefreshTokenAsync(request.RefreshToken);
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
			});
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in {Method}", nameof(RefreshToken));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost("logout")]
	[Authorize]
	[EndpointSummary("Logout and invalidate refresh token")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> Logout()
	{
		try
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
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in {Method}", nameof(Logout));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost("change-password")]
	[Authorize]
	[EndpointSummary("Change password (required on first login)")]
	[ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<TokenResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
	{
		try
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
				return BadRequest(result.Errors.Select(e => e.Description));
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
			});
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in {Method}", nameof(ChangePassword));
			return StatusCode(500, "An error occurred while processing your request.");
		}
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
