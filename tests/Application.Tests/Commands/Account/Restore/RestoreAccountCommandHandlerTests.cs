using Application.Commands.Account.Restore;
using Application.Interfaces.Services;
using Moq;

namespace Application.Tests.Commands.Account.Restore;

public class RestoreAccountCommandHandlerTests
{
	[Fact]
	public async Task Handle_ExistingDeletedEntity_ReturnsTrue()
	{
		// Arrange
		Mock<IAccountService> mockService = new();
		RestoreAccountCommandHandler handler = new(mockService.Object);
		Guid id = Guid.NewGuid();
		mockService.Setup(s => s.RestoreAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
		RestoreAccountCommand command = new(id);

		// Act
		bool result = await handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public async Task Handle_NonExistentEntity_ReturnsFalse()
	{
		// Arrange
		Mock<IAccountService> mockService = new();
		RestoreAccountCommandHandler handler = new(mockService.Object);
		Guid id = Guid.NewGuid();
		mockService.Setup(s => s.RestoreAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
		RestoreAccountCommand command = new(id);

		// Act
		bool result = await handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.False(result);
	}
}
