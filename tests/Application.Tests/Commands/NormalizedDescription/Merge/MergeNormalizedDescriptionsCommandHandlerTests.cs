using Application.Commands.NormalizedDescription.Merge;
using Application.Interfaces.Services;
using FluentAssertions;
using Moq;

namespace Application.Tests.Commands.NormalizedDescription.Merge;

public class MergeNormalizedDescriptionsCommandHandlerTests
{
	[Fact]
	public async Task Handle_ForwardsIdsToServiceAndReturnsRelinkCount()
	{
		// Arrange
		Mock<INormalizedDescriptionService> mockService = new();
		Guid keepId = Guid.NewGuid();
		Guid discardId = Guid.NewGuid();

		mockService
			.Setup(s => s.MergeAsync(keepId, discardId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(7);

		MergeNormalizedDescriptionsCommandHandler handler = new(mockService.Object);
		MergeNormalizedDescriptionsCommand command = new(keepId, discardId);

		// Act
		int result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().Be(7);
		mockService.Verify(s => s.MergeAsync(keepId, discardId, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_MissingRowReturnsZero_ReflectsServiceContract()
	{
		// The service returns 0 when either id is missing or both ids are equal. The handler
		// just forwards that count — it's the controller's job to validate inputs before call.
		Mock<INormalizedDescriptionService> mockService = new();
		Guid keepId = Guid.NewGuid();
		Guid discardId = Guid.NewGuid();

		mockService
			.Setup(s => s.MergeAsync(keepId, discardId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(0);

		MergeNormalizedDescriptionsCommandHandler handler = new(mockService.Object);
		MergeNormalizedDescriptionsCommand command = new(keepId, discardId);

		int result = await handler.Handle(command, CancellationToken.None);

		result.Should().Be(0);
	}
}
