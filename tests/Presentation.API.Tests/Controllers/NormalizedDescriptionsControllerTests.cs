using API.Controllers;
using API.Generated.Dtos;
using Application.Commands.NormalizedDescription.UpdateSettings;
using Application.Models.NormalizedDescriptions;
using Application.Queries.NormalizedDescription.GetSettings;
using Application.Queries.NormalizedDescription.PreviewThresholdImpact;
using Application.Queries.NormalizedDescription.TestMatch;
using Domain.NormalizedDescriptions;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace Presentation.API.Tests.Controllers;

public class NormalizedDescriptionsControllerTests
{
	private readonly Mock<IMediator> _mediatorMock;
	private readonly NormalizedDescriptionsController _controller;

	public NormalizedDescriptionsControllerTests()
	{
		_mediatorMock = new Mock<IMediator>();
		_controller = new NormalizedDescriptionsController(_mediatorMock.Object);
	}

	// ── GET settings ────────────────────────────────────────────

	[Fact]
	public async Task GetSettings_ReturnsOkWithMappedResponse()
	{
		NormalizedDescriptionSettings settings = new(
			Guid.NewGuid(), 0.81, 0.68,
			new DateTimeOffset(2026, 4, 19, 0, 0, 0, TimeSpan.Zero));

		_mediatorMock
			.Setup(m => m.Send(It.IsAny<GetNormalizedDescriptionSettingsQuery>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(settings);

		Ok<NormalizedDescriptionSettingsResponse> result = await _controller.GetSettings(CancellationToken.None);

		result.Value!.Id.Should().Be(settings.Id);
		result.Value.AutoAcceptThreshold.Should().Be(0.81);
		result.Value.PendingReviewThreshold.Should().Be(0.68);
	}

	// ── PATCH settings ──────────────────────────────────────────

	[Fact]
	public async Task UpdateSettings_ValidRequest_ReturnsOkWithUpdatedValues()
	{
		UpdateNormalizedDescriptionSettingsRequest request = new()
		{
			AutoAcceptThreshold = 0.9,
			PendingReviewThreshold = 0.5,
		};

		NormalizedDescriptionSettings updated = new(
			Guid.NewGuid(), 0.9, 0.5,
			new DateTimeOffset(2026, 4, 19, 0, 0, 0, TimeSpan.Zero));

		_mediatorMock
			.Setup(m => m.Send(
				It.Is<UpdateNormalizedDescriptionSettingsCommand>(c => c.AutoAcceptThreshold == 0.9 && c.PendingReviewThreshold == 0.5),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(updated);

		Results<Ok<NormalizedDescriptionSettingsResponse>, BadRequest<string>> result =
			await _controller.UpdateSettings(request, CancellationToken.None);

		Ok<NormalizedDescriptionSettingsResponse> ok = Assert.IsType<Ok<NormalizedDescriptionSettingsResponse>>(result.Result);
		ok.Value!.AutoAcceptThreshold.Should().Be(0.9);
		ok.Value.PendingReviewThreshold.Should().Be(0.5);
	}

	[Theory]
	[InlineData(-0.01, 0.5, NormalizedDescriptionsController.AutoAcceptOutOfRange)]
	[InlineData(1.01, 0.5, NormalizedDescriptionsController.AutoAcceptOutOfRange)]
	[InlineData(0.8, -0.01, NormalizedDescriptionsController.PendingReviewOutOfRange)]
	[InlineData(0.8, 1.01, NormalizedDescriptionsController.PendingReviewOutOfRange)]
	[InlineData(0.5, 0.6, NormalizedDescriptionsController.PendingMustBeLessThanAuto)]
	[InlineData(0.5, 0.5, NormalizedDescriptionsController.PendingMustBeLessThanAuto)]
	public async Task UpdateSettings_InvalidRequest_ReturnsBadRequest(double autoAccept, double pendingReview, string expectedMessage)
	{
		UpdateNormalizedDescriptionSettingsRequest request = new()
		{
			AutoAcceptThreshold = autoAccept,
			PendingReviewThreshold = pendingReview,
		};

		Results<Ok<NormalizedDescriptionSettingsResponse>, BadRequest<string>> result =
			await _controller.UpdateSettings(request, CancellationToken.None);

		BadRequest<string> bad = Assert.IsType<BadRequest<string>>(result.Result);
		bad.Value.Should().Be(expectedMessage);

		// Should short-circuit without invoking the mediator.
		_mediatorMock.Verify(m => m.Send(It.IsAny<UpdateNormalizedDescriptionSettingsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	// ── POST test ───────────────────────────────────────────────

	[Fact]
	public async Task TestMatch_ValidRequest_ReturnsOkWithCandidates()
	{
		TestMatchRequest request = new()
		{
			Description = "whole milk",
			TopN = 3,
			AutoAcceptThresholdOverride = 0.85,
			PendingReviewThresholdOverride = 0.55,
		};

		Guid candidateId = Guid.NewGuid();
		MatchTestResult serviceResult = new(
			Candidates: [new MatchCandidate(candidateId, "Whole Milk", 0.92, "Active")],
			SimulatedOutcome: MatchTestOutcomes.AutoAccept,
			SimulatedTargetId: candidateId);

		_mediatorMock
			.Setup(m => m.Send(
				It.Is<TestNormalizedDescriptionMatchQuery>(q =>
					q.Description == "whole milk" &&
					q.TopN == 3 &&
					q.AutoAcceptThresholdOverride == 0.85 &&
					q.PendingReviewThresholdOverride == 0.55),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(serviceResult);

		Results<Ok<MatchTestResultResponse>, BadRequest<string>> result =
			await _controller.TestMatch(request, CancellationToken.None);

		Ok<MatchTestResultResponse> ok = Assert.IsType<Ok<MatchTestResultResponse>>(result.Result);
		ok.Value!.SimulatedOutcome.Should().Be("AutoAccept");
		ok.Value.SimulatedTargetId.Should().Be(candidateId);
		ok.Value.Candidates.Should().ContainSingle();
		MatchCandidateDto first = ok.Value.Candidates.First();
		first.CanonicalName.Should().Be("Whole Milk");
		first.CosineSimilarity.Should().Be(0.92);
	}

	[Fact]
	public async Task TestMatch_ZeroTopN_DefaultsToFive()
	{
		// The generated DTO has TopN default = 5, but JSON deserialization from a client
		// that omits TopN (or sends topN=0 explicitly) can set it to 0. The controller
		// coerces 0 → 5 as a defensive fallback.
		TestMatchRequest request = new()
		{
			Description = "milk",
			TopN = 0,
		};

		_mediatorMock
			.Setup(m => m.Send(It.Is<TestNormalizedDescriptionMatchQuery>(q => q.TopN == 5), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new MatchTestResult([], MatchTestOutcomes.CreateNew, null));

		Results<Ok<MatchTestResultResponse>, BadRequest<string>> result =
			await _controller.TestMatch(request, CancellationToken.None);

		Assert.IsType<Ok<MatchTestResultResponse>>(result.Result);
		_mediatorMock.Verify(m => m.Send(It.Is<TestNormalizedDescriptionMatchQuery>(q => q.TopN == 5), It.IsAny<CancellationToken>()), Times.Once);
	}

	[Theory]
	[InlineData("", NormalizedDescriptionsController.DescriptionRequired)]
	[InlineData("   ", NormalizedDescriptionsController.DescriptionRequired)]
	public async Task TestMatch_EmptyDescription_ReturnsBadRequest(string description, string expectedMessage)
	{
		TestMatchRequest request = new() { Description = description, TopN = 5 };

		Results<Ok<MatchTestResultResponse>, BadRequest<string>> result =
			await _controller.TestMatch(request, CancellationToken.None);

		BadRequest<string> bad = Assert.IsType<BadRequest<string>>(result.Result);
		bad.Value.Should().Be(expectedMessage);
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(21)]
	public async Task TestMatch_TopNOutOfRange_ReturnsBadRequest(int topN)
	{
		TestMatchRequest request = new() { Description = "milk", TopN = topN };

		Results<Ok<MatchTestResultResponse>, BadRequest<string>> result =
			await _controller.TestMatch(request, CancellationToken.None);

		BadRequest<string> bad = Assert.IsType<BadRequest<string>>(result.Result);
		bad.Value.Should().Be(NormalizedDescriptionsController.TopNOutOfRange);
	}

	[Fact]
	public async Task TestMatch_OverrideOutOfRange_ReturnsBadRequest()
	{
		TestMatchRequest request = new()
		{
			Description = "milk",
			TopN = 5,
			AutoAcceptThresholdOverride = 1.5,
		};

		Results<Ok<MatchTestResultResponse>, BadRequest<string>> result =
			await _controller.TestMatch(request, CancellationToken.None);

		BadRequest<string> bad = Assert.IsType<BadRequest<string>>(result.Result);
		bad.Value.Should().Be(NormalizedDescriptionsController.OverrideOutOfRange);
	}

	[Fact]
	public async Task TestMatch_CrossedOverrides_ReturnsBadRequest()
	{
		TestMatchRequest request = new()
		{
			Description = "milk",
			TopN = 5,
			AutoAcceptThresholdOverride = 0.5,
			PendingReviewThresholdOverride = 0.8,
		};

		Results<Ok<MatchTestResultResponse>, BadRequest<string>> result =
			await _controller.TestMatch(request, CancellationToken.None);

		BadRequest<string> bad = Assert.IsType<BadRequest<string>>(result.Result);
		bad.Value.Should().Be(NormalizedDescriptionsController.PendingMustBeLessThanAuto);
	}

	// ── POST settings/preview ────────────────────────────────────

	[Fact]
	public async Task PreviewThresholdImpact_ValidRequest_ReturnsOkWithMappedCounts()
	{
		PreviewThresholdImpactRequest request = new()
		{
			AutoAcceptThreshold = 0.75,
			PendingReviewThreshold = 0.4,
		};

		ThresholdImpactPreview preview = new(
			Current: new ClassificationCounts(10, 5, 3),
			Proposed: new ClassificationCounts(15, 2, 1),
			Deltas: new ReclassificationDeltas(AutoToPending: 0, PendingToAuto: 3, UnresolvedToAuto: 2, UnresolvedToPending: 0));

		_mediatorMock
			.Setup(m => m.Send(
				It.Is<PreviewThresholdImpactQuery>(q => q.AutoAcceptThreshold == 0.75 && q.PendingReviewThreshold == 0.4),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(preview);

		Results<Ok<ThresholdImpactPreviewResponse>, BadRequest<string>> result =
			await _controller.PreviewThresholdImpact(request, CancellationToken.None);

		Ok<ThresholdImpactPreviewResponse> ok = Assert.IsType<Ok<ThresholdImpactPreviewResponse>>(result.Result);
		ok.Value!.Current.AutoAccepted.Should().Be(10);
		ok.Value.Proposed.AutoAccepted.Should().Be(15);
		ok.Value.Deltas.PendingToAuto.Should().Be(3);
		ok.Value.Deltas.UnresolvedToAuto.Should().Be(2);
	}

	[Theory]
	[InlineData(-0.1, 0.5, NormalizedDescriptionsController.AutoAcceptOutOfRange)]
	[InlineData(1.1, 0.5, NormalizedDescriptionsController.AutoAcceptOutOfRange)]
	[InlineData(0.8, -0.1, NormalizedDescriptionsController.PendingReviewOutOfRange)]
	[InlineData(0.8, 1.1, NormalizedDescriptionsController.PendingReviewOutOfRange)]
	[InlineData(0.5, 0.6, NormalizedDescriptionsController.PendingMustBeLessThanAuto)]
	public async Task PreviewThresholdImpact_InvalidRequest_ReturnsBadRequest(double autoAccept, double pendingReview, string expectedMessage)
	{
		PreviewThresholdImpactRequest request = new()
		{
			AutoAcceptThreshold = autoAccept,
			PendingReviewThreshold = pendingReview,
		};

		Results<Ok<ThresholdImpactPreviewResponse>, BadRequest<string>> result =
			await _controller.PreviewThresholdImpact(request, CancellationToken.None);

		BadRequest<string> bad = Assert.IsType<BadRequest<string>>(result.Result);
		bad.Value.Should().Be(expectedMessage);

		_mediatorMock.Verify(m => m.Send(It.IsAny<PreviewThresholdImpactQuery>(), It.IsAny<CancellationToken>()), Times.Never);
	}
}
