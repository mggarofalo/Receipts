using Application.Commands.Transaction;
using Application.Interfaces.Repositories;
using SampleData.Domain.Core;
using Moq;

namespace Application.Tests.Commands.Transaction;

public class CreateTransactionCommandHandlerTests
{
	[Fact]
	public async Task CreateTransactionCommandHandler_WithValidCommand_ReturnsCreatedTransactions()
	{
		Mock<ITransactionRepository> mockRepository = new();
		CreateTransactionCommandHandler handler = new(mockRepository.Object);

		List<Domain.Core.Transaction> input = TransactionGenerator.GenerateList(2);

		mockRepository.Setup(r => r
			.CreateAsync(It.IsAny<List<Domain.Core.Transaction>>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(input);

		CreateTransactionCommand command = new(input, Guid.NewGuid(), Guid.NewGuid());
		List<Domain.Core.Transaction> result = await handler.Handle(command, CancellationToken.None);

		Assert.Equal(input.Count, result.Count);
	}
}