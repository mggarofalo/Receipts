using Application.Queries.Transaction;
using Application.Interfaces.Repositories;
using Domain;
using Moq;

namespace Application.Tests.Queries.Transaction;

public class GetAllTransactionsQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnAllTransactions()
	{
		List<Domain.Core.Transaction> allTransactions =
		[
			new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new Money(100), new DateOnly(2021, 1, 1)),
			new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new Money(200), new DateOnly(2021, 1, 2))
		];

		Mock<ITransactionRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(allTransactions);

		GetAllTransactionsQueryHandler handler = new(mockRepository.Object);
		GetAllTransactionsQuery query = new();

		List<Domain.Core.Transaction> result = await handler.Handle(query, CancellationToken.None);

		Assert.Equal(allTransactions.Count, result.Count);
		Assert.True(allTransactions.All(t => result.Any(rt =>
			rt.Id == t.Id &&
			rt.ReceiptId == t.ReceiptId &&
			rt.AccountId == t.AccountId &&
			rt.Amount == t.Amount &&
			rt.Date == t.Date)));

		mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}