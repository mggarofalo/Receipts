using API.Generated.Dtos;
using Application.Interfaces.Services;
using Infrastructure.Entities;
using Infrastructure.Entities.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
[Authorize(Policy = "RequireAdmin")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class UsersController(
	IUserService userService,
	UserManager<ApplicationUser> userManager,
	IAuthAuditService authAuditService,
	ILogger<UsersController> logger) : ControllerBase
{
	[HttpGet]
	[EndpointSummary("List all users with their roles")]
	[ProducesResponseType<UserListResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<UserListResponse>> ListUsers(
		[FromQuery] int page = 1,
		[FromQuery] int pageSize = 20)
	{
		try
		{
			PagedUserList result = await userService.ListUsersAsync(page, pageSize);

			List<UserSummaryResponse> items = result.Items.Select(MapToResponse).ToList();

			return Ok(new UserListResponse
			{
				Items = items,
				Page = result.Page,
				PageSize = result.PageSize,
				TotalCount = result.TotalCount,
			});
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in {Method}", nameof(ListUsers));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet("{userId}")]
	[EndpointSummary("Get a user by ID")]
	[ProducesResponseType<UserSummaryResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<UserSummaryResponse>> GetUser(string userId)
	{
		try
		{
			ApplicationUser? user = await userManager.FindByIdAsync(userId);
			if (user is null)
			{
				return NotFound();
			}

			IList<string> roles = await userManager.GetRolesAsync(user);

			return Ok(new UserSummaryResponse
			{
				Id = user.Id,
				Email = user.Email ?? "",
				FirstName = user.FirstName,
				LastName = user.LastName,
				Roles = [.. roles],
				IsDisabled = user.LockoutEnabled && user.LockoutEnd > DateTimeOffset.UtcNow,
				CreatedAt = user.CreatedAt,
				LastLoginAt = user.LastLoginAt,
			});
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in {Method}", nameof(GetUser));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost]
	[EndpointSummary("Create a new user (admin only)")]
	[ProducesResponseType<UserSummaryResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<UserSummaryResponse>> CreateUser([FromBody] CreateUserRequest request)
	{
		try
		{
			ApplicationUser user = new()
			{
				UserName = request.Email,
				Email = request.Email,
				FirstName = request.FirstName,
				LastName = request.LastName,
				MustResetPassword = true,
				CreatedAt = DateTimeOffset.UtcNow,
			};

			IdentityResult result = await userManager.CreateAsync(user, request.Password);
			if (!result.Succeeded)
			{
				return BadRequest(result.Errors.Select(e => e.Description));
			}

			await userManager.AddToRoleAsync(user, request.Role);

			await LogAuthEventAsync(nameof(AuthEventType.UserRegistered), user.Id, user.Email);

			return Ok(new UserSummaryResponse
			{
				Id = user.Id,
				Email = user.Email!,
				FirstName = user.FirstName,
				LastName = user.LastName,
				Roles = [request.Role],
				IsDisabled = false,
				CreatedAt = user.CreatedAt,
				LastLoginAt = null,
			});
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in {Method}", nameof(CreateUser));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPut("{userId}")]
	[EndpointSummary("Update a user (admin only)")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
	{
		try
		{
			string? currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			ApplicationUser? user = await userManager.FindByIdAsync(userId);
			if (user is null)
			{
				return NotFound();
			}

			if (userId == currentUserId)
			{
				if (request.IsDisabled)
				{
					return BadRequest("Cannot disable your own account.");
				}

				IList<string> currentRoles = await userManager.GetRolesAsync(user);
				if (currentRoles.Contains("Admin") && request.Role != "Admin")
				{
					return BadRequest("Cannot remove your own Admin role.");
				}
			}

			user.Email = request.Email;
			user.UserName = request.Email;
			user.FirstName = request.FirstName;
			user.LastName = request.LastName;

			if (request.IsDisabled)
			{
				user.LockoutEnabled = true;
				user.LockoutEnd = DateTimeOffset.MaxValue;
			}
			else
			{
				user.LockoutEnd = null;
			}

			await userManager.UpdateAsync(user);

			IList<string> roles = await userManager.GetRolesAsync(user);
			if (roles.Count > 0)
			{
				await userManager.RemoveFromRolesAsync(user, roles);
			}
			await userManager.AddToRoleAsync(user, request.Role);

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in {Method}", nameof(UpdateUser));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpDelete("{userId}")]
	[EndpointSummary("Deactivate a user (admin only)")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> DeactivateUser(string userId)
	{
		try
		{
			string? currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == currentUserId)
			{
				return BadRequest("Cannot deactivate your own account.");
			}

			ApplicationUser? user = await userManager.FindByIdAsync(userId);
			if (user is null)
			{
				return NotFound();
			}

			user.LockoutEnabled = true;
			user.LockoutEnd = DateTimeOffset.MaxValue;
			user.RefreshToken = null;
			user.RefreshTokenExpiresAt = null;
			await userManager.UpdateAsync(user);

			await LogAuthEventAsync(nameof(AuthEventType.AccountDisabled), user.Id, user.Email);

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in {Method}", nameof(DeactivateUser));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost("{userId}/reset-password")]
	[EndpointSummary("Reset a user's password (admin only)")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> AdminResetPassword(string userId, [FromBody] AdminResetPasswordRequest request)
	{
		try
		{
			ApplicationUser? user = await userManager.FindByIdAsync(userId);
			if (user is null)
			{
				return NotFound();
			}

			await userManager.RemovePasswordAsync(user);
			IdentityResult result = await userManager.AddPasswordAsync(user, request.NewPassword);
			if (!result.Succeeded)
			{
				return BadRequest(result.Errors.Select(e => e.Description));
			}

			user.MustResetPassword = true;
			user.RefreshToken = null;
			user.RefreshTokenExpiresAt = null;
			await userManager.UpdateAsync(user);

			await LogAuthEventAsync(nameof(AuthEventType.PasswordChanged), user.Id, user.Email);

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in {Method}", nameof(AdminResetPassword));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	private static UserSummaryResponse MapToResponse(UserSummary user) => new()
	{
		Id = user.Id,
		Email = user.Email,
		FirstName = user.FirstName,
		LastName = user.LastName,
		Roles = [.. user.Roles],
		IsDisabled = user.IsDisabled,
		CreatedAt = user.CreatedAt,
		LastLoginAt = user.LastLoginAt,
	};

	private async Task LogAuthEventAsync(string eventType, string? userId, string? username)
	{
		try
		{
			await authAuditService.LogAsync(new AuthAuditEntryDto(
				Guid.NewGuid(),
				eventType,
				userId,
				null,
				username,
				true,
				null,
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
