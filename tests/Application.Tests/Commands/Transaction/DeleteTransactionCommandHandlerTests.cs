using Application.Commands.Transaction;
using SampleData.Domain.Core;
using Moq;
using Application.Interfaces.Services;

namespace Application.Tests.Commands.Transaction;

public class DeleteTransactionCommandHandlerTests
{
	[Fact]
	public async Task DeleteTransactionCommandHandler_WithValidCommand_ReturnsTrueAndCallsDeleteAndSaveChanges()
	{
		Mock<ITransactionService> mockService = new();
		DeleteTransactionCommandHandler handler = new(mockService.Object);

		List<Guid> input = TransactionGenerator.GenerateList(2).Select(t => t.Id!.Value).ToList();

		DeleteTransactionCommand command = new(input);
		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);
	}
}