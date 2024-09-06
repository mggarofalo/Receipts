using Application.Commands.Transaction;
using Application.Interfaces.Repositories;
using SampleData.Domain.Core;
using Moq;

namespace Application.Tests.Commands.Transaction;

public class UpdateTransactionCommandHandlerTests
{
	[Fact]
	public async Task UpdateTransactionCommandHandler_WithValidCommand_ReturnsTrueAndCallsUpdateAndSaveChanges()
	{
		Mock<ITransactionRepository> mockRepository = new();
		UpdateTransactionCommandHandler handler = new(mockRepository.Object);

		List<Domain.Core.Transaction> input = TransactionGenerator.GenerateList(2);

		mockRepository.Setup(r => r
			.UpdateAsync(It.IsAny<List<Domain.Core.Transaction>>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		UpdateTransactionCommand command = new(input);
		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);
	}
}