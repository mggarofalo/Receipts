using Application.Commands.Subcategory.Restore;
using Application.Interfaces.Services;
using Moq;

namespace Application.Tests.Commands.Subcategory.Restore;

public class RestoreSubcategoryCommandHandlerTests
{
	[Fact]
	public async Task Handle_ExistingDeletedEntity_ReturnsTrue()
	{
		// Arrange
		Mock<ISubcategoryService> mockService = new();
		RestoreSubcategoryCommandHandler handler = new(mockService.Object);
		Guid id = Guid.NewGuid();
		mockService.Setup(s => s.RestoreAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
		RestoreSubcategoryCommand command = new(id);

		// Act
		bool result = await handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public async Task Handle_NonExistentEntity_ReturnsFalse()
	{
		// Arrange
		Mock<ISubcategoryService> mockService = new();
		RestoreSubcategoryCommandHandler handler = new(mockService.Object);
		Guid id = Guid.NewGuid();
		mockService.Setup(s => s.RestoreAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
		RestoreSubcategoryCommand command = new(id);

		// Act
		bool result = await handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.False(result);
	}
}
