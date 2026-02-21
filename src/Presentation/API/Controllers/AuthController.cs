using API.Generated.Dtos;
using Application.Interfaces.Services;
using Common;
using Infrastructure.Entities;
using Infrastructure.Entities.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController(
	UserManager<ApplicationUser> userManager,
	ITokenService tokenService,
	IAuthAuditService authAuditService,
	ILogger<AuthController> logger) : ControllerBase
{
	[HttpPost("register")]
	[AllowAnonymous]
	[EndpointSummary("Register a new user")]
	[ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<TokenResponse>> Register([FromBody] RegisterRequest request)
	{
		try
		{
			ApplicationUser user = new()
			{
				UserName = request.Email,
				Email = request.Email,
				FirstName = request.FirstName,
				LastName = request.LastName,
			};

			IdentityResult result = await userManager.CreateAsync(user, request.Password);
			if (!result.Succeeded)
			{
				return BadRequest(result.Errors.Select(e => e.Description));
			}

			await userManager.AddToRoleAsync(user, AppRoles.User);

			IList<string> roles = await userManager.GetRolesAsync(user);
			string accessToken = tokenService.GenerateAccessToken(user.Id, user.Email!, roles);
			string refreshToken = tokenService.GenerateRefreshToken();

			user.RefreshToken = refreshToken;
			user.RefreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(30);
			await userManager.UpdateAsync(user);

			await LogAuthEventAsync(nameof(AuthEventType.UserRegistered), user.Id, user.Email, true);

			return Ok(new TokenResponse
			{
				AccessToken = accessToken,
				RefreshToken = refreshToken,
				ExpiresIn = 3600,
			});
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in {Method}", nameof(Register));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

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

			IList<string> roles = await userManager.GetRolesAsync(user);
			string accessToken = tokenService.GenerateAccessToken(user.Id, user.Email!, roles);
			string refreshToken = tokenService.GenerateRefreshToken();

			user.RefreshToken = refreshToken;
			user.RefreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(30);
			await userManager.UpdateAsync(user);

			await LogAuthEventAsync(nameof(AuthEventType.Login), user.Id, user.Email, true);

			return Ok(new TokenResponse
			{
				AccessToken = accessToken,
				RefreshToken = refreshToken,
				ExpiresIn = 3600,
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
			ApplicationUser? user = await userManager.Users
				.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

			if (user is null
				|| user.RefreshTokenExpiresAt is null
				|| user.RefreshTokenExpiresAt < DateTimeOffset.UtcNow)
			{
				return Unauthorized();
			}

			IList<string> roles = await userManager.GetRolesAsync(user);
			string accessToken = tokenService.GenerateAccessToken(user.Id, user.Email!, roles);
			string newRefreshToken = tokenService.GenerateRefreshToken();

			user.RefreshToken = newRefreshToken;
			user.RefreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(30);
			await userManager.UpdateAsync(user);

			return Ok(new TokenResponse
			{
				AccessToken = accessToken,
				RefreshToken = newRefreshToken,
				ExpiresIn = 3600,
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
