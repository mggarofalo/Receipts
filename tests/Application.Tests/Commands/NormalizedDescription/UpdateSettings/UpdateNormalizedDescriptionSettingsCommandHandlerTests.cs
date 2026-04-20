using Application.Commands.NormalizedDescription.UpdateSettings;
using Application.Interfaces.Services;
using Domain.NormalizedDescriptions;
using FluentAssertions;
using Moq;

namespace Application.Tests.Commands.NormalizedDescription.UpdateSettings;

public class UpdateNormalizedDescriptionSettingsCommandHandlerTests
{
	[Fact]
	public async Task Handle_ForwardsThresholdsToServiceAndReturnsResult()
	{
		// Arrange
		Mock<INormalizedDescriptionService> mockService = new();
		NormalizedDescriptionSettings expected = new(
			Guid.NewGuid(),
			autoAcceptThreshold: 0.9,
			pendingReviewThreshold: 0.5,
			updatedAt: new DateTimeOffset(2026, 4, 19, 0, 0, 0, TimeSpan.Zero));

		mockService
			.Setup(s => s.UpdateSettingsAsync(0.9, 0.5, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		UpdateNormalizedDescriptionSettingsCommandHandler handler = new(mockService.Object);
		UpdateNormalizedDescriptionSettingsCommand command = new(0.9, 0.5);

		// Act
		NormalizedDescriptionSettings actual = await handler.Handle(command, CancellationToken.None);

		// Assert
		actual.Should().BeSameAs(expected);
		mockService.Verify(s => s.UpdateSettingsAsync(0.9, 0.5, It.IsAny<CancellationToken>()), Times.Once);
	}
}
