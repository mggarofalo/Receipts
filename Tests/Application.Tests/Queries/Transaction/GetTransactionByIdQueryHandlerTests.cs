using Application.Interfaces.Repositories;
using Application.Queries.Transaction;
using SampleData.Domain.Core;
using Moq;

namespace Application.Tests.Queries.Transaction;

public class GetTransactionByIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnTransaction_WhenTransactionExists()
	{
		Domain.Core.Transaction transaction = TransactionGenerator.Generate();

		Mock<ITransactionRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(transaction);

		GetTransactionByIdQueryHandler handler = new(mockRepository.Object);
		GetTransactionByIdQuery query = new(transaction.Id!.Value);
		Domain.Core.Transaction? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
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
		mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
	}
}