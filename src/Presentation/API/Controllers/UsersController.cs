using System.Security.Claims;
using API.Generated.Dtos;
using Application.Interfaces.Services;
using Application.Models;
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
[Route("api/users")]
[Produces("application/json")]
[Authorize(Policy = "RequireAdmin")]
public class UsersController(
	IUserService userService,
	UserManager<ApplicationUser> userManager,
	IAuthAuditService authAuditService,
	ILogger<UsersController> logger) : ControllerBase
{
	[HttpGet]
	[EndpointSummary("List all users with their roles")]
	public async Task<Ok<UserListResponse>> ListUsers(
		[FromQuery] int offset = 0,
		[FromQuery] int limit = 50)
	{
		PagedResult<UserSummary> result = await userService.ListUsersAsync(offset, limit);

		List<UserSummaryResponse> items = result.Data.Select(MapToResponse).ToList();

		return TypedResults.Ok(new UserListResponse
		{
			Data = items,
			Total = result.Total,
			Offset = result.Offset,
			Limit = result.Limit,
		});
	}

	[HttpGet("{userId}")]
	[EndpointSummary("Get a user by ID")]
	public async Task<Results<Ok<UserSummaryResponse>, NotFound>> GetUser(string userId)
	{
		ApplicationUser? user = await userManager.FindByIdAsync(userId);
		if (user is null)
		{
			return TypedResults.NotFound();
		}

		IList<string> roles = await userManager.GetRolesAsync(user);

		return TypedResults.Ok(new UserSummaryResponse
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

	[HttpPost]
	[EndpointSummary("Create a new user (admin only)")]
	public async Task<Results<Ok<UserSummaryResponse>, BadRequest<IEnumerable<string>>>> CreateUser([FromBody] CreateUserRequest request)
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
			return TypedResults.BadRequest(result.Errors.Select(e => e.Description));
		}

		IdentityResult roleResult = await userManager.AddToRoleAsync(user, request.Role);
		if (!roleResult.Succeeded)
		{
			return TypedResults.BadRequest(roleResult.Errors.Select(e => e.Description));
		}

		await LogAuthEventAsync(nameof(AuthEventType.UserRegistered), user.Id, user.Email);

		return TypedResults.Ok(new UserSummaryResponse
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

	[HttpPut("{userId}")]
	[EndpointSummary("Update a user (admin only)")]
	public async Task<Results<NoContent, NotFound, BadRequest<string>, BadRequest<IEnumerable<string>>>> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
	{
		string? currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

		ApplicationUser? user = await userManager.FindByIdAsync(userId);
		if (user is null)
		{
			return TypedResults.NotFound();
		}

		if (userId == currentUserId)
		{
			if (request.IsDisabled)
			{
				return TypedResults.BadRequest("Cannot disable your own account.");
			}

			IList<string> currentRoles = await userManager.GetRolesAsync(user);
			if (currentRoles.Contains("Admin") && request.Role != "Admin")
			{
				return TypedResults.BadRequest("Cannot remove your own Admin role.");
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
			user.LockoutEnabled = false;
			user.LockoutEnd = null;
		}

		IdentityResult updateResult = await userManager.UpdateAsync(user);
		if (!updateResult.Succeeded)
		{
			return TypedResults.BadRequest(updateResult.Errors.Select(e => e.Description));
		}

		IList<string> roles = await userManager.GetRolesAsync(user);
		if (roles.Count > 0)
		{
			await userManager.RemoveFromRolesAsync(user, roles);
		}

		IdentityResult roleResult = await userManager.AddToRoleAsync(user, request.Role);
		if (!roleResult.Succeeded)
		{
			return TypedResults.BadRequest(roleResult.Errors.Select(e => e.Description));
		}

		return TypedResults.NoContent();
	}

	[HttpDelete("{userId}")]
	[EndpointSummary("Deactivate a user (admin only)")]
	public async Task<Results<NoContent, BadRequest<string>, NotFound>> DeactivateUser(string userId)
	{
		string? currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (userId == currentUserId)
		{
			return TypedResults.BadRequest("Cannot deactivate your own account.");
		}

		ApplicationUser? user = await userManager.FindByIdAsync(userId);
		if (user is null)
		{
			return TypedResults.NotFound();
		}

		user.LockoutEnabled = true;
		user.LockoutEnd = DateTimeOffset.MaxValue;
		user.RefreshToken = null;
		user.RefreshTokenExpiresAt = null;
		await userManager.UpdateAsync(user);

		await LogAuthEventAsync(nameof(AuthEventType.AccountDisabled), user.Id, user.Email);

		return TypedResults.NoContent();
	}

	[HttpPost("{userId}/reset-password")]
	[EndpointSummary("Reset a user's password (admin only)")]
	public async Task<Results<NoContent, NotFound, BadRequest<IEnumerable<string>>>> AdminResetPassword(string userId, [FromBody] AdminResetPasswordRequest request)
	{
		ApplicationUser? user = await userManager.FindByIdAsync(userId);
		if (user is null)
		{
			return TypedResults.NotFound();
		}

		await userManager.RemovePasswordAsync(user);
		IdentityResult result = await userManager.AddPasswordAsync(user, request.NewPassword);
		if (!result.Succeeded)
		{
			return TypedResults.BadRequest(result.Errors.Select(e => e.Description));
		}

		user.MustResetPassword = true;
		user.RefreshToken = null;
		user.RefreshTokenExpiresAt = null;
		await userManager.UpdateAsync(user);

		await LogAuthEventAsync(nameof(AuthEventType.PasswordChanged), user.Id, user.Email);

		return TypedResults.NoContent();
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
			await authAuditService.LogAsync(new Application.Interfaces.Services.AuthAuditEntryDto(
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
