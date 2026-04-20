using Application.Commands.NormalizedDescription.UpdateStatus;
using Application.Interfaces.Services;
using Domain.NormalizedDescriptions;
using FluentAssertions;
using Moq;

namespace Application.Tests.Commands.NormalizedDescription.UpdateStatus;

public class UpdateNormalizedDescriptionStatusCommandHandlerTests
{
	[Fact]
	public async Task Handle_StatusChanged_ReturnsTrue()
	{
		// Arrange
		Mock<INormalizedDescriptionService> mockService = new();
		Guid id = Guid.NewGuid();

		mockService
			.Setup(s => s.UpdateStatusAsync(id, NormalizedDescriptionStatus.Active, It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		UpdateNormalizedDescriptionStatusCommandHandler handler = new(mockService.Object);
		UpdateNormalizedDescriptionStatusCommand command = new(id, NormalizedDescriptionStatus.Active);

		// Act
		bool result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().BeTrue();
		mockService.Verify(s => s.UpdateStatusAsync(id, NormalizedDescriptionStatus.Active, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_RowMissingOrAlreadyAtStatus_ReturnsFalse()
	{
		// The service returns false for both "row missing" and "row already at target" cases.
		// The handler forwards without distinguishing — it's the controller's job to do an
		// existence check first to distinguish 404 from idempotent no-op.
		Mock<INormalizedDescriptionService> mockService = new();
		Guid id = Guid.NewGuid();

		mockService
			.Setup(s => s.UpdateStatusAsync(id, NormalizedDescriptionStatus.PendingReview, It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		UpdateNormalizedDescriptionStatusCommandHandler handler = new(mockService.Object);
		UpdateNormalizedDescriptionStatusCommand command = new(id, NormalizedDescriptionStatus.PendingReview);

		bool result = await handler.Handle(command, CancellationToken.None);

		result.Should().BeFalse();
	}
}
