using Application.Interfaces.Repositories;
using Application.Queries.Transaction;
using SampleData.Domain.Core;
using Moq;

namespace Application.Tests.Queries.Transaction;

public class GetAllTransactionsQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnAllTransactions()
	{
		List<Domain.Core.Transaction> transactions = TransactionGenerator.GenerateList(2);

		Mock<ITransactionRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transactions);

		GetAllTransactionsQueryHandler handler = new(mockRepository.Object);
		GetAllTransactionsQuery query = new();

		List<Domain.Core.Transaction> result = await handler.Handle(query, CancellationToken.None);

		Assert.Equal(transactions.Count, result.Count);
		Assert.True(transactions.All(input => result.Any(output =>
			output.Id == input.Id &&
			output.ReceiptId == input.ReceiptId &&
			output.AccountId == input.AccountId &&
			output.Amount == input.Amount &&
			output.Date == input.Date)));

		mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}