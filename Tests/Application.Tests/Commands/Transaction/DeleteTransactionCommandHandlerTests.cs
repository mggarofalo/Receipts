using Application.Commands.Transaction;
using Application.Interfaces.Repositories;
using SampleData.Domain.Core;
using Moq;

namespace Application.Tests.Commands.Transaction;

public class DeleteTransactionCommandHandlerTests
{
	[Fact]
	public async Task DeleteTransactionCommandHandler_WithValidCommand_ReturnsTrueAndCallsDeleteAndSaveChanges()
	{
		Mock<ITransactionRepository> mockRepository = new();
		DeleteTransactionCommandHandler handler = new(mockRepository.Object);

		List<Guid> input = TransactionGenerator.GenerateList(2).Select(t => t.Id!.Value).ToList();

		DeleteTransactionCommand command = new(input);
		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);
	}
}