using API.Generated.Dtos;
using Common;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/users/{userId}/roles")]
[Produces("application/json")]
[Authorize(Policy = "RequireAdmin")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class UserRolesController(
	UserManager<ApplicationUser> userManager,
	ILogger<UserRolesController> logger) : ControllerBase
{
	[HttpGet]
	[EndpointSummary("List roles for a user")]
	[ProducesResponseType<UserRolesResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<UserRolesResponse>> GetUserRoles([FromRoute] string userId)
	{
		try
		{
			ApplicationUser? user = await userManager.FindByIdAsync(userId);
			if (user is null)
			{
				return NotFound();
			}

			IList<string> roles = await userManager.GetRolesAsync(user);
			return Ok(new UserRolesResponse
			{
				Roles = [.. roles],
			});
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in {Method} for userId: {UserId}", nameof(GetUserRoles), userId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost("{role}")]
	[EndpointSummary("Assign role to user")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> AssignUserRole([FromRoute] string userId, [FromRoute] string role)
	{
		try
		{
			if (!AppRoles.All.Contains(role))
			{
				return BadRequest($"Invalid role '{role}'. Valid roles: {string.Join(", ", AppRoles.All)}");
			}

			ApplicationUser? user = await userManager.FindByIdAsync(userId);
			if (user is null)
			{
				return NotFound();
			}

			await userManager.AddToRoleAsync(user, role);
			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in {Method} for userId: {UserId}, role: {Role}", nameof(AssignUserRole), userId, role);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpDelete("{role}")]
	[EndpointSummary("Remove role from user")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> RemoveUserRole([FromRoute] string userId, [FromRoute] string role)
	{
		try
		{
			if (!AppRoles.All.Contains(role))
			{
				return BadRequest($"Invalid role '{role}'. Valid roles: {string.Join(", ", AppRoles.All)}");
			}

			ApplicationUser? user = await userManager.FindByIdAsync(userId);
			if (user is null)
			{
				return NotFound();
			}

			await userManager.RemoveFromRoleAsync(user, role);
			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in {Method} for userId: {UserId}, role: {Role}", nameof(RemoveUserRole), userId, role);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}
}
