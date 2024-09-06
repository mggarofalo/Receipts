using Application.Interfaces.Repositories;
using SampleData.Domain.Core;
using Moq;
using Application.Queries.Core.Transaction;

namespace Application.Tests.Queries.Core.Transaction;

public class GetAllTransactionsQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnAllTransactions()
	{
		List<Domain.Core.Transaction> expected = TransactionGenerator.GenerateList(2);

		Mock<ITransactionRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetAllTransactionsQueryHandler handler = new(mockRepository.Object);
		GetAllTransactionsQuery query = new();

		List<Domain.Core.Transaction> result = await handler.Handle(query, CancellationToken.None);

		Assert.Equal(expected.Count, result.Count);
		Assert.True(expected.All(result.Contains));
		Assert.True(result.All(expected.Contains));
	}
}