using API.Generated.Dtos;
using Application.Commands.NormalizedDescription.UpdateSettings;
using Application.Models.NormalizedDescriptions;
using Application.Queries.NormalizedDescription.GetSettings;
using Application.Queries.NormalizedDescription.PreviewThresholdImpact;
using Application.Queries.NormalizedDescription.TestMatch;
using Asp.Versioning;
using Domain.NormalizedDescriptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

// RECEIPTS-580 scaffolds this controller with the admin settings/test/preview endpoints
// only; RECEIPTS-579 will extend the same class with merge/split/status/list/get endpoints.
// All endpoints gate on RequireAdmin — tuning thresholds and probing the classifier are
// operator tasks, not end-user ones.
[ApiVersion("1.0")]
[ApiController]
[Route("api/normalized-descriptions")]
[Produces("application/json")]
[Authorize(Policy = "RequireAdmin")]
public class NormalizedDescriptionsController(IMediator mediator) : ControllerBase
{
	public const string RouteSettings = "settings";
	public const string RoutePreview = "settings/preview";
	public const string RouteTest = "test";

	public const string AutoAcceptOutOfRange = "autoAcceptThreshold must be between 0 and 1";
	public const string PendingReviewOutOfRange = "pendingReviewThreshold must be between 0 and 1";
	public const string PendingMustBeLessThanAuto = "pendingReviewThreshold must be strictly less than autoAcceptThreshold";
	public const string DescriptionRequired = "description must not be empty";
	public const string TopNOutOfRange = "topN must be between 1 and 20";
	public const string OverrideOutOfRange = "threshold overrides must be between 0 and 1 when provided";

	[HttpGet(RouteSettings)]
	[EndpointSummary("Get the current normalized-description threshold settings")]
	[EndpointDescription("Returns the live auto-accept and pending-review thresholds used by the resolver. Admin-only.")]
	public async Task<Ok<NormalizedDescriptionSettingsResponse>> GetSettings(CancellationToken cancellationToken)
	{
		NormalizedDescriptionSettings settings = await mediator.Send(new GetNormalizedDescriptionSettingsQuery(), cancellationToken);
		return TypedResults.Ok(ToResponse(settings));
	}

	[HttpPatch(RouteSettings)]
	[EndpointSummary("Update the normalized-description threshold settings")]
	[EndpointDescription("Both thresholds must satisfy 0 <= pendingReviewThreshold < autoAcceptThreshold <= 1. Admin-only.")]
	public async Task<Results<Ok<NormalizedDescriptionSettingsResponse>, BadRequest<string>>> UpdateSettings(
		[FromBody] UpdateNormalizedDescriptionSettingsRequest request,
		CancellationToken cancellationToken)
	{
		if (request.AutoAcceptThreshold < 0 || request.AutoAcceptThreshold > 1)
		{
			return TypedResults.BadRequest(AutoAcceptOutOfRange);
		}

		if (request.PendingReviewThreshold < 0 || request.PendingReviewThreshold > 1)
		{
			return TypedResults.BadRequest(PendingReviewOutOfRange);
		}

		if (request.PendingReviewThreshold >= request.AutoAcceptThreshold)
		{
			return TypedResults.BadRequest(PendingMustBeLessThanAuto);
		}

		UpdateNormalizedDescriptionSettingsCommand command = new(request.AutoAcceptThreshold, request.PendingReviewThreshold);
		NormalizedDescriptionSettings result = await mediator.Send(command, cancellationToken);
		return TypedResults.Ok(ToResponse(result));
	}

	[HttpPost(RouteTest)]
	[EndpointSummary("Probe the normalized-description classifier for a given description")]
	[EndpointDescription("Returns the top-N ANN candidates and the classification branch the resolver would take. Admin-only.")]
	public async Task<Results<Ok<MatchTestResultResponse>, BadRequest<string>>> TestMatch(
		[FromBody] TestMatchRequest request,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(request.Description))
		{
			return TypedResults.BadRequest(DescriptionRequired);
		}

		int topN = request.TopN == 0 ? 5 : request.TopN;
		if (topN < 1 || topN > 20)
		{
			return TypedResults.BadRequest(TopNOutOfRange);
		}

		if (request.AutoAcceptThresholdOverride is double autoOverride &&
			(autoOverride < 0 || autoOverride > 1))
		{
			return TypedResults.BadRequest(OverrideOutOfRange);
		}

		if (request.PendingReviewThresholdOverride is double pendingOverride &&
			(pendingOverride < 0 || pendingOverride > 1))
		{
			return TypedResults.BadRequest(OverrideOutOfRange);
		}

		// If both overrides are supplied, reject a crossed pair up front for the same reason
		// UpdateSettings does — don't wait for the service to throw.
		if (request.AutoAcceptThresholdOverride is double autoSet &&
			request.PendingReviewThresholdOverride is double pendingSet &&
			pendingSet >= autoSet)
		{
			return TypedResults.BadRequest(PendingMustBeLessThanAuto);
		}

		TestNormalizedDescriptionMatchQuery query = new(
			request.Description,
			topN,
			request.AutoAcceptThresholdOverride,
			request.PendingReviewThresholdOverride);

		MatchTestResult result = await mediator.Send(query, cancellationToken);
		return TypedResults.Ok(ToResponse(result));
	}

	[HttpPost(RoutePreview)]
	[EndpointSummary("Preview the impact of changing normalized-description thresholds")]
	[EndpointDescription("Returns current and proposed classification counts with per-bucket deltas. Admin-only.")]
	public async Task<Results<Ok<ThresholdImpactPreviewResponse>, BadRequest<string>>> PreviewThresholdImpact(
		[FromBody] PreviewThresholdImpactRequest request,
		CancellationToken cancellationToken)
	{
		if (request.AutoAcceptThreshold < 0 || request.AutoAcceptThreshold > 1)
		{
			return TypedResults.BadRequest(AutoAcceptOutOfRange);
		}

		if (request.PendingReviewThreshold < 0 || request.PendingReviewThreshold > 1)
		{
			return TypedResults.BadRequest(PendingReviewOutOfRange);
		}

		if (request.PendingReviewThreshold >= request.AutoAcceptThreshold)
		{
			return TypedResults.BadRequest(PendingMustBeLessThanAuto);
		}

		PreviewThresholdImpactQuery query = new(request.AutoAcceptThreshold, request.PendingReviewThreshold);
		ThresholdImpactPreview result = await mediator.Send(query, cancellationToken);
		return TypedResults.Ok(ToResponse(result));
	}

	private static NormalizedDescriptionSettingsResponse ToResponse(NormalizedDescriptionSettings settings) => new()
	{
		Id = settings.Id,
		AutoAcceptThreshold = settings.AutoAcceptThreshold,
		PendingReviewThreshold = settings.PendingReviewThreshold,
		UpdatedAt = settings.UpdatedAt,
	};

	private static MatchTestResultResponse ToResponse(MatchTestResult result) => new()
	{
		SimulatedOutcome = result.SimulatedOutcome,
		SimulatedTargetId = result.SimulatedTargetId,
		Candidates = [.. result.Candidates.Select(c => new MatchCandidateDto
		{
			NormalizedDescriptionId = c.NormalizedDescriptionId,
			CanonicalName = c.CanonicalName,
			CosineSimilarity = c.CosineSimilarity,
			Status = c.Status,
		})],
	};

	private static ThresholdImpactPreviewResponse ToResponse(ThresholdImpactPreview preview) => new()
	{
		Current = ToResponse(preview.Current),
		Proposed = ToResponse(preview.Proposed),
		Deltas = new ReclassificationDeltasDto
		{
			AutoToPending = preview.Deltas.AutoToPending,
			PendingToAuto = preview.Deltas.PendingToAuto,
			UnresolvedToAuto = preview.Deltas.UnresolvedToAuto,
			UnresolvedToPending = preview.Deltas.UnresolvedToPending,
		},
	};

	private static ClassificationCountsDto ToResponse(ClassificationCounts counts) => new()
	{
		AutoAccepted = counts.AutoAccepted,
		PendingReview = counts.PendingReview,
		Unresolved = counts.Unresolved,
	};
}
