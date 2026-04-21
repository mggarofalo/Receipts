using Application.Interfaces.Services;
using Application.Models.NormalizedDescriptions;
using Application.Queries.NormalizedDescription.PreviewThresholdImpact;
using FluentAssertions;
using Moq;

namespace Application.Tests.Queries.NormalizedDescription.PreviewThresholdImpact;

public class PreviewThresholdImpactQueryHandlerTests
{
	[Fact]
	public async Task Handle_ForwardsThresholdsToServiceAndReturnsResult()
	{
		Mock<INormalizedDescriptionService> mockService = new();
		ThresholdImpactPreview expected = new(
			Current: new ClassificationCounts(10, 5, 2),
			Proposed: new ClassificationCounts(12, 3, 2),
			Deltas: new ReclassificationDeltas(AutoToPending: 0, PendingToAuto: 2, UnresolvedToAuto: 0, UnresolvedToPending: 0));

		mockService
			.Setup(s => s.PreviewThresholdImpactAsync(0.75, 0.5, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		PreviewThresholdImpactQueryHandler handler = new(mockService.Object);
		PreviewThresholdImpactQuery query = new(0.75, 0.5);

		ThresholdImpactPreview actual = await handler.Handle(query, CancellationToken.None);

		actual.Should().BeSameAs(expected);
		mockService.Verify(s => s.PreviewThresholdImpactAsync(0.75, 0.5, It.IsAny<CancellationToken>()), Times.Once);
	}
}
