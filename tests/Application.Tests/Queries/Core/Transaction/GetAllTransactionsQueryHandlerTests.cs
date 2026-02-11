using SampleData.Domain.Core;
using Moq;
using Application.Queries.Core.Transaction;
using Application.Interfaces.Services;
using FluentAssertions;

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

		result.Should().BeSameAs(expected);
	}
}