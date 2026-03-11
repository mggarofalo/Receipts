using API.Generated.Dtos;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/metadata")]
[Authorize]
public class MetadataController : ControllerBase
{
	private static readonly EnumMetadataResponse CachedResponse = new()
	{
		AdjustmentTypes = EnumLabels.AdjustmentTypes,
		AuthEventTypes = EnumLabels.AuthEventTypes,
		PricingModes = EnumLabels.PricingModes,
		AuditActions = EnumLabels.AuditActions,
		EntityTypes = EnumLabels.EntityTypes,
	};

	[HttpGet("enums")]
	[EndpointSummary("Get all enum values with display labels")]
	public Ok<EnumMetadataResponse> GetEnums()
	{
		return TypedResults.Ok(CachedResponse);
	}
}
