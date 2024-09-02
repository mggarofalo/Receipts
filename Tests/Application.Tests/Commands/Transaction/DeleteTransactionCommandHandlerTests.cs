using Application.Commands.Transaction;
using Application.Interfaces.Repositories;
using Moq;

namespace Application.Tests.Commands.Transaction;

public class DeleteTransactionCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsTrueAndCallsDeleteAndSaveChanges()
	{
		Mock<ITransactionRepository> mockRepository = new();
		DeleteTransactionCommandHandler handler = new(mockRepository.Object);

		List<Guid> inputTransactionIds =
		[
			Guid.NewGuid(),
			Guid.NewGuid()
		];

		DeleteTransactionCommand command = new(inputTransactionIds);

		await handler.Handle(command, CancellationToken.None);

		mockRepository.Verify(r => r.DeleteAsync(It.Is<List<Guid>>(ids =>
			ids.Count() == inputTransactionIds.Count &&
			ids.All(id => inputTransactionIds.Any(i => id == i))),
			It.IsAny<CancellationToken>()), Times.Once);

		mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}