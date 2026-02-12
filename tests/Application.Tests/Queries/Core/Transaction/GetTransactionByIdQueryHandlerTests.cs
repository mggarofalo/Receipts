using Application.Interfaces.Services;
using Application.Queries.Core.Transaction;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Core.Transaction;

public class GetTransactionByIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnTransaction_WhenTransactionExists()
	{
		Domain.Core.Transaction expected = TransactionGenerator.Generate();

		Mock<ITransactionService> mockService = new();
		mockService.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetTransactionByIdQueryHandler handler = new(mockService.Object);
		GetTransactionByIdQuery query = new(expected.Id);
		Domain.Core.Transaction? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		result.Should().BeSameAs(expected);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenTransactionDoesNotExist()
	{
		Mock<ITransactionService> mockService = new();
		mockService.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.Transaction?)null);

		GetTransactionByIdQueryHandler handler = new(mockService.Object);
		GetTransactionByIdQuery query = new(Guid.NewGuid());
		Domain.Core.Transaction? result = await handler.Handle(query, CancellationToken.None);

		Assert.Null(result);
	}
}