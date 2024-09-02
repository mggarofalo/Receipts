using Application.Commands.Transaction;
using Application.Interfaces.Repositories;
using SampleData.Domain.Core;
using Moq;

namespace Application.Tests.Commands.Transaction;

public class DeleteTransactionCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsTrueAndCallsDeleteAndSaveChanges()
	{
		Mock<ITransactionRepository> mockRepository = new();
		DeleteTransactionCommandHandler handler = new(mockRepository.Object);

		List<Guid> input = TransactionGenerator.GenerateList(2).Select(t => t.Id!.Value).ToList();

		DeleteTransactionCommand command = new(input);
		await handler.Handle(command, CancellationToken.None);

		mockRepository.Verify(r => r.DeleteAsync(It.Is<List<Guid>>(ids =>
			ids.Count() == input.Count &&
			ids.All(id => input.Any(i => id == i))),
			It.IsAny<CancellationToken>()), Times.Once);

		mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}