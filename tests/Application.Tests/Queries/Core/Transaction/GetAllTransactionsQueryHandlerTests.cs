using SampleData.Domain.Core;
using Moq;
using Application.Queries.Core.Transaction;
using Application.Interfaces.Services;

namespace Application.Tests.Queries.Core.Transaction;

public class GetAllTransactionsQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnAllTransactions()
	{
		List<Domain.Core.Transaction> expected = TransactionGenerator.GenerateList(2);

		Mock<ITransactionService> mockService = new();
		mockService.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetAllTransactionsQueryHandler handler = new(mockService.Object);
		GetAllTransactionsQuery query = new();

		List<Domain.Core.Transaction> result = await handler.Handle(query, CancellationToken.None);

		Assert.Equal(expected.Count, result.Count);
		Assert.True(expected.All(result.Contains));
		Assert.True(result.All(expected.Contains));
	}
}