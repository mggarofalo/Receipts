using Application.Interfaces.Services;
using Application.Models;
using Application.Queries.Core.Transaction;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Core.Transaction;

public class GetAllTransactionsQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnAllTransactions()
	{
		List<Domain.Core.Transaction> expected = TransactionGenerator.GenerateList(2);

		Mock<ITransactionService> mockService = new();
		mockService.Setup(r => r.GetAllAsync(0, 50, It.IsAny<CancellationToken>())).ReturnsAsync(new PagedResult<Domain.Core.Transaction>(expected, expected.Count, 0, 50));

		GetAllTransactionsQueryHandler handler = new(mockService.Object);
		GetAllTransactionsQuery query = new(0, 50);

		PagedResult<Domain.Core.Transaction> result = await handler.Handle(query, CancellationToken.None);

		result.Data.Should().BeSameAs(expected);
	}
}
