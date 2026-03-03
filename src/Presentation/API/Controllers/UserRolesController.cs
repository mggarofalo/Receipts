using API.Generated.Dtos;
using Asp.Versioning;
using Common;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/users/{userId}/roles")]
[Produces("application/json")]
[Authorize(Policy = "RequireAdmin")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class UserRolesController(
	UserManager<ApplicationUser> userManager) : ControllerBase
{
	[HttpGet]
	[EndpointSummary("List roles for a user")]
	[ProducesResponseType<UserRolesResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<UserRolesResponse>> GetUserRoles([FromRoute] string userId)
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

	[HttpPost("{role}")]
	[EndpointSummary("Assign role to user")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> AssignUserRole([FromRoute] string userId, [FromRoute] string role)
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

	[HttpDelete("{role}")]
	[EndpointSummary("Remove role from user")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> RemoveUserRole([FromRoute] string userId, [FromRoute] string role)
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
}
