using API.Generated.Dtos;
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
	UserManager<ApplicationUser> userManager,
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

			int totalCount = await userManager.Users.CountAsync();

			List<ApplicationUser> users = await userManager.Users
				.OrderBy(u => u.Email)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			List<UserSummaryResponse> items = [];
			foreach (ApplicationUser user in users)
			{
				IList<string> roles = await userManager.GetRolesAsync(user);
				items.Add(new UserSummaryResponse
				{
					Id = user.Id,
					Email = user.Email ?? "",
					Roles = [.. roles],
				});
			}

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
