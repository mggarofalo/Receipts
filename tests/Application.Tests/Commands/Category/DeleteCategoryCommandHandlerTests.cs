using Application.Commands.Category.Delete;
using Application.Interfaces.Services;
using Moq;

namespace Application.Tests.Commands.Category;

public class DeleteCategoryCommandHandlerTests
{
	[Fact]
	public async Task Handle_WhenCategoryExists_ReturnsTrueAndCallsDelete()
	{
		// Arrange
		Mock<ICategoryService> mockService = new();
		DeleteCategoryCommandHandler handler = new(mockService.Object);
		Guid id = Guid.NewGuid();

		mockService.Setup(s => s.ExistsAsync(id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);
		mockService.Setup(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		DeleteCategoryCommand command = new(id);

		// Act
		bool result = await handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.True(result);
		mockService.Verify(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WhenCategoryDoesNotExist_ReturnsFalse()
	{
		// Arrange
		Mock<ICategoryService> mockService = new();
		DeleteCategoryCommandHandler handler = new(mockService.Object);
		Guid id = Guid.NewGuid();

		mockService.Setup(s => s.ExistsAsync(id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		DeleteCategoryCommand command = new(id);

		// Act
		bool result = await handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.False(result);
		mockService.Verify(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
	}
}
