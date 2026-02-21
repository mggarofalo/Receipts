using Application.Commands.ReceiptItem.Restore;
using Application.Interfaces.Services;
using Moq;

namespace Application.Tests.Commands.ReceiptItem.Restore;

public class RestoreReceiptItemCommandHandlerTests
{
	[Fact]
	public async Task Handle_ExistingDeletedEntity_ReturnsTrue()
	{
		// Arrange
		Mock<IReceiptItemService> mockService = new();
		RestoreReceiptItemCommandHandler handler = new(mockService.Object);
		Guid id = Guid.NewGuid();
		mockService.Setup(s => s.RestoreAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
		RestoreReceiptItemCommand command = new(id);

		// Act
		bool result = await handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public async Task Handle_NonExistentEntity_ReturnsFalse()
	{
		// Arrange
		Mock<IReceiptItemService> mockService = new();
		RestoreReceiptItemCommandHandler handler = new(mockService.Object);
		Guid id = Guid.NewGuid();
		mockService.Setup(s => s.RestoreAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
		RestoreReceiptItemCommand command = new(id);

		// Act
		bool result = await handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.False(result);
	}
}
