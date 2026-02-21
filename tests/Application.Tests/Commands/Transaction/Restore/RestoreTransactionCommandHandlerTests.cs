using Application.Commands.Transaction.Restore;
using Application.Interfaces.Services;
using Moq;

namespace Application.Tests.Commands.Transaction.Restore;

public class RestoreTransactionCommandHandlerTests
{
	[Fact]
	public async Task Handle_ExistingDeletedEntity_ReturnsTrue()
	{
		// Arrange
		Mock<ITransactionService> mockService = new();
		RestoreTransactionCommandHandler handler = new(mockService.Object);
		Guid id = Guid.NewGuid();
		mockService.Setup(s => s.RestoreAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
		RestoreTransactionCommand command = new(id);

		// Act
		bool result = await handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public async Task Handle_NonExistentEntity_ReturnsFalse()
	{
		// Arrange
		Mock<ITransactionService> mockService = new();
		RestoreTransactionCommandHandler handler = new(mockService.Object);
		Guid id = Guid.NewGuid();
		mockService.Setup(s => s.RestoreAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
		RestoreTransactionCommand command = new(id);

		// Act
		bool result = await handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.False(result);
	}
}
