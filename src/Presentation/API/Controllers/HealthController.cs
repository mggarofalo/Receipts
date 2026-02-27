using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/health")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
	[HttpGet]
	[EndpointSummary("Check API health")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public IActionResult Get()
	{
		return Ok(new { status = "Healthy" });
	}
}
