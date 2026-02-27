using Application.Commands.Trash.Purge;
using Application.Interfaces.Services;
using Moq;

namespace Application.Tests.Commands.Trash;

public class PurgeTrashCommandHandlerTests
{
	[Fact]
	public async Task Handle_CallsPurgeAllDeletedAsync()
	{
		Mock<ITrashService> mockService = new();
		PurgeTrashCommandHandler handler = new(mockService.Object);

		PurgeTrashCommand command = new();
		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);
		mockService.Verify(s => s.PurgeAllDeletedAsync(CancellationToken.None), Times.Once);
	}
}
