using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
	[HttpGet]
	[EndpointSummary("Check API health")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public IActionResult Get()
	{
		return Ok(new { status = "Hello there." });
	}
}
