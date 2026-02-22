using API.Generated.Dtos;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
[Authorize(Policy = "RequireAdmin")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class UsersController(
	IUserService userService,
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

			List<UserSummaryResponse> items = result.Items.Select(user => new UserSummaryResponse
			{
				Id = user.Id,
				Email = user.Email,
				Roles = [.. user.Roles],
			}).ToList();

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
}
