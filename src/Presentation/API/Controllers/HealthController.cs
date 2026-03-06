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
	public async Task<IResult> Get(
		[FromServices] ApplicationDbContext dbContext,
		CancellationToken cancellationToken)
	{
		bool dbOk = await dbContext.Database.CanConnectAsync(cancellationToken);
		var payload = new { status = dbOk ? "Healthy" : "Unhealthy", database = dbOk };
		return dbOk
			? TypedResults.Ok<object>(payload)
			: Results.Json(payload, statusCode: 503);
	}
}
