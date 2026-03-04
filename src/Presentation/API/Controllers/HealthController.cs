using Asp.Versioning;
using Infrastructure;
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
	public async Task<Ok<object>> Get(
		[FromServices] ApplicationDbContext dbContext,
		CancellationToken cancellationToken)
	{
		bool dbOk = await dbContext.Database.CanConnectAsync(cancellationToken);
		return TypedResults.Ok<object>(new { status = "Healthy", database = dbOk });
	}
}
