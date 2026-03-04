using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/health")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
	[HttpGet]
	[EndpointSummary("Check API health")]
	public Ok<object> Get()
	{
		return TypedResults.Ok<object>(new { status = "Healthy" });
	}
}
