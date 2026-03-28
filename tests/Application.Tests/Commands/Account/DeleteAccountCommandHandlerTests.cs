using Application.Commands.Account.Delete;
using Application.Interfaces.Services;
using Moq;

namespace Application.Tests.Commands.Account;

public class DeleteAccountCommandHandlerTests
{
	[Fact]
	public async Task Handle_WhenAccountExists_ReturnsTrueAndCallsDelete()
	{
		// Arrange
		Mock<IAccountService> mockService = new();
		DeleteAccountCommandHandler handler = new(mockService.Object);
		Guid id = Guid.NewGuid();

		mockService.Setup(s => s.ExistsAsync(id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);
		mockService.Setup(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		DeleteAccountCommand command = new(id);

		// Act
		bool result = await handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.True(result);
		mockService.Verify(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WhenAccountDoesNotExist_ReturnsFalse()
	{
		// Arrange
		Mock<IAccountService> mockService = new();
		DeleteAccountCommandHandler handler = new(mockService.Object);
		Guid id = Guid.NewGuid();

		mockService.Setup(s => s.ExistsAsync(id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		DeleteAccountCommand command = new(id);

		// Act
		bool result = await handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.False(result);
		mockService.Verify(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
	}
}
