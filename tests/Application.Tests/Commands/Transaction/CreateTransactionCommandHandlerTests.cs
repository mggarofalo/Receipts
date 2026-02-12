using Application.Commands.Transaction.Create;
using Application.Interfaces.Services;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Transaction;

public class CreateTransactionCommandHandlerTests
{
	[Fact]
	public async Task CreateTransactionCommandHandler_WithValidCommand_ReturnsCreatedTransactions()
	{
		Mock<ITransactionService> mockService = new();
		CreateTransactionCommandHandler handler = new(mockService.Object);

		List<Domain.Core.Transaction> input = TransactionGenerator.GenerateList(2);

		mockService.Setup(r => r
			.CreateAsync(It.IsAny<List<Domain.Core.Transaction>>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(input);

		CreateTransactionCommand command = new(input, Guid.NewGuid(), Guid.NewGuid());
		List<Domain.Core.Transaction> result = await handler.Handle(command, CancellationToken.None);

		result.Should().HaveCount(input.Count);
	}
}