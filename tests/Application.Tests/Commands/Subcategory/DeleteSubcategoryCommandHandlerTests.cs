using Application.Commands.Subcategory.Delete;
using Application.Interfaces.Services;
using Moq;

namespace Application.Tests.Commands.Subcategory;

public class DeleteSubcategoryCommandHandlerTests
{
	[Fact]
	public async Task Handle_WhenSubcategoryExists_ReturnsTrueAndCallsDelete()
	{
		// Arrange
		Mock<ISubcategoryService> mockService = new();
		DeleteSubcategoryCommandHandler handler = new(mockService.Object);
		Guid id = Guid.NewGuid();

		mockService.Setup(s => s.ExistsAsync(id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);
		mockService.Setup(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		DeleteSubcategoryCommand command = new(id);

		// Act
		bool result = await handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.True(result);
		mockService.Verify(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WhenSubcategoryDoesNotExist_ReturnsFalse()
	{
		// Arrange
		Mock<ISubcategoryService> mockService = new();
		DeleteSubcategoryCommandHandler handler = new(mockService.Object);
		Guid id = Guid.NewGuid();

		mockService.Setup(s => s.ExistsAsync(id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		DeleteSubcategoryCommand command = new(id);

		// Act
		bool result = await handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.False(result);
		mockService.Verify(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
	}
}
