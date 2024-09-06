using Application.Interfaces.Repositories;
using SampleData.Domain.Core;
using Moq;
using Application.Queries.Core.Transaction;

namespace Application.Tests.Queries.Core.Transaction;

public class GetTransactionByIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnTransaction_WhenTransactionExists()
	{
		Domain.Core.Transaction expected = TransactionGenerator.Generate();

		Mock<ITransactionRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetTransactionByIdQueryHandler handler = new(mockRepository.Object);
		GetTransactionByIdQuery query = new(expected.Id!.Value);
		Domain.Core.Transaction? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		Assert.Equal(expected, result);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenTransactionDoesNotExist()
	{
		Mock<ITransactionRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.Transaction?)null);

		GetTransactionByIdQueryHandler handler = new(mockRepository.Object);
		GetTransactionByIdQuery query = new(Guid.NewGuid());
		Domain.Core.Transaction? result = await handler.Handle(query, CancellationToken.None);

		Assert.Null(result);
	}
}