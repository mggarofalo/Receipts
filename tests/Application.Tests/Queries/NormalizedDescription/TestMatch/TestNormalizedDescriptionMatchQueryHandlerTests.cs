using Application.Interfaces.Services;
using Application.Models.NormalizedDescriptions;
using Application.Queries.NormalizedDescription.TestMatch;
using FluentAssertions;
using Moq;

namespace Application.Tests.Queries.NormalizedDescription.TestMatch;

public class TestNormalizedDescriptionMatchQueryHandlerTests
{
	[Fact]
	public async Task Handle_ForwardsParametersToServiceAndReturnsResult()
	{
		Mock<INormalizedDescriptionService> mockService = new();
		MatchTestResult expected = new(
			Candidates: [new MatchCandidate(Guid.NewGuid(), "Milk", 0.92, "Active")],
			SimulatedOutcome: MatchTestOutcomes.AutoAccept,
			SimulatedTargetId: Guid.NewGuid());

		mockService
			.Setup(s => s.TestMatchAsync("whole milk", 3, 0.85, 0.55, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		TestNormalizedDescriptionMatchQueryHandler handler = new(mockService.Object);
		TestNormalizedDescriptionMatchQuery query = new("whole milk", 3, 0.85, 0.55);

		MatchTestResult actual = await handler.Handle(query, CancellationToken.None);

		actual.Should().BeSameAs(expected);
		mockService.Verify(s => s.TestMatchAsync("whole milk", 3, 0.85, 0.55, It.IsAny<CancellationToken>()), Times.Once);
	}
}
