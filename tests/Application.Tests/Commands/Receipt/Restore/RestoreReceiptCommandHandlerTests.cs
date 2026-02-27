using Application.Commands.Receipt.Restore;
using Application.Interfaces.Services;
using Moq;

namespace Application.Tests.Commands.Receipt.Restore;

public class RestoreReceiptCommandHandlerTests
{
	[Fact]
	public async Task Handle_ExistingDeletedEntity_ReturnsTrue()
	{
		// Arrange
		Mock<IReceiptService> mockService = new();
		RestoreReceiptCommandHandler handler = new(mockService.Object);
		Guid id = Guid.NewGuid();
		mockService.Setup(s => s.RestoreAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
		RestoreReceiptCommand command = new(id);

		// Act
		bool result = await handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public async Task Handle_NonExistentEntity_ReturnsFalse()
	{
		// Arrange
		Mock<IReceiptService> mockService = new();
		RestoreReceiptCommandHandler handler = new(mockService.Object);
		Guid id = Guid.NewGuid();
		mockService.Setup(s => s.RestoreAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
		RestoreReceiptCommand command = new(id);

		// Act
		bool result = await handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.False(result);
	}
}
