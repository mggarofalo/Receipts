using Application.Commands.Card.Delete;
using Application.Interfaces.Services;
using Moq;

namespace Application.Tests.Commands.Card;

public class DeleteCardCommandHandlerTests
{
	[Fact]
	public async Task Handle_WhenAccountExists_ReturnsTrueAndCallsDelete()
	{
		// Arrange
		Mock<ICardService> mockService = new();
		DeleteCardCommandHandler handler = new(mockService.Object);
		Guid id = Guid.NewGuid();

		mockService.Setup(s => s.ExistsAsync(id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);
		mockService.Setup(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		DeleteCardCommand command = new(id);

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
		Mock<ICardService> mockService = new();
		DeleteCardCommandHandler handler = new(mockService.Object);
		Guid id = Guid.NewGuid();

		mockService.Setup(s => s.ExistsAsync(id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		DeleteCardCommand command = new(id);

		// Act
		bool result = await handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.False(result);
		mockService.Verify(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
	}
}
