using API.Generated.Dtos;
using Infrastructure;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
[Authorize(Policy = "RequireAdmin")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class UsersController(
	ApplicationDbContext dbContext,
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
			page = Math.Max(1, page);
			pageSize = Math.Clamp(pageSize, 1, 100);

			int totalCount = await dbContext.Users.CountAsync();

			List<ApplicationUser> users = await dbContext.Users
				.OrderBy(u => u.Email)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			List<string> userIds = users.Select(u => u.Id).ToList();

			Dictionary<string, List<string>> rolesByUserId = await dbContext.UserRoles
				.Where(ur => userIds.Contains(ur.UserId))
				.Join(
					dbContext.Roles,
					ur => ur.RoleId,
					r => r.Id,
					(ur, r) => new { ur.UserId, RoleName = r.Name! })
				.GroupBy(x => x.UserId)
				.ToDictionaryAsync(
					g => g.Key,
					g => g.Select(x => x.RoleName).ToList());

			List<UserSummaryResponse> items = users.Select(user => new UserSummaryResponse
			{
				Id = user.Id,
				Email = user.Email ?? "",
				Roles = [.. rolesByUserId.GetValueOrDefault(user.Id, [])],
			}).ToList();

			return Ok(new UserListResponse
			{
				Items = items,
				Page = page,
				PageSize = pageSize,
				TotalCount = totalCount,
			});
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in {Method}", nameof(ListUsers));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}
}
