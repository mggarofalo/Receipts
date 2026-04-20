using Application.Interfaces.Services;
using Application.Queries.NormalizedDescription.GetSettings;
using Domain.NormalizedDescriptions;
using FluentAssertions;
using Moq;

namespace Application.Tests.Queries.NormalizedDescription.GetSettings;

public class GetNormalizedDescriptionSettingsQueryHandlerTests
{
	[Fact]
	public async Task Handle_ReturnsSettingsFromService()
	{
		Mock<INormalizedDescriptionService> mockService = new();
		NormalizedDescriptionSettings expected = new(
			Guid.NewGuid(),
			autoAcceptThreshold: 0.81,
			pendingReviewThreshold: 0.68,
			updatedAt: new DateTimeOffset(2026, 4, 19, 0, 0, 0, TimeSpan.Zero));

		mockService
			.Setup(s => s.GetSettingsAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		GetNormalizedDescriptionSettingsQueryHandler handler = new(mockService.Object);
		GetNormalizedDescriptionSettingsQuery query = new();

		NormalizedDescriptionSettings actual = await handler.Handle(query, CancellationToken.None);

		actual.Should().BeSameAs(expected);
		mockService.Verify(s => s.GetSettingsAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}
