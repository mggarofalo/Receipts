using Application.Commands.Transaction;
using Application.Interfaces.Repositories;
using Domain;
using Moq;

namespace Application.Tests.Commands.Transaction;

public class CreateTransactionCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsCreatedTransactions()
	{
		Mock<ITransactionRepository> mockRepository = new();
		CreateTransactionCommandHandler handler = new(mockRepository.Object);

		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();

		List<Domain.Core.Transaction> inputTransactions =
		[
			new(null, receiptId, accountId, new Money(100), new DateOnly(2021, 1, 1)),
			new(null, receiptId, accountId, new Money(200), new DateOnly(2021, 1, 2))
		];

		CreateTransactionCommand command = new(inputTransactions);

		List<Domain.Core.Transaction> createdTransactions =
		[
			new(Guid.NewGuid(), receiptId, accountId, new Money(100), new DateOnly(2021, 1, 1)),
			new(Guid.NewGuid(), receiptId, accountId, new Money(200), new DateOnly(2021, 1, 2))
		];

		mockRepository.Setup(r => r
			.CreateAsync(It.IsAny<List<Domain.Core.Transaction>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(createdTransactions);

		List<Domain.Core.Transaction> result = await handler.Handle(command, CancellationToken.None);

		Assert.Equal(createdTransactions.Count, result.Count);
		Assert.Equal(createdTransactions, result);

		mockRepository.Verify(r => r.CreateAsync(It.Is<List<Domain.Core.Transaction>>(transactions =>
			transactions.Count() == inputTransactions.Count &&
			transactions.All(t => inputTransactions.Any(it =>
				it.ReceiptId == t.ReceiptId &&
				it.AccountId == t.AccountId &&
				it.Amount == t.Amount &&
				it.Date == t.Date))),
			It.IsAny<CancellationToken>()), Times.Once);

		mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}