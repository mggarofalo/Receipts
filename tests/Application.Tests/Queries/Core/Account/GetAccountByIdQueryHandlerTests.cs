using Application.Interfaces.Repositories;
using Application.Queries.Core.Account;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Core.Account;

public class GetAccountByIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnAccount_WhenAccountExists()
	{
		Domain.Core.Account expected = AccountGenerator.Generate();

		Mock<IAccountRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetAccountByIdQueryHandler handler = new(mockRepository.Object);
		GetAccountByIdQuery query = new(expected.Id!.Value);
		Domain.Core.Account? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		Assert.Equal(expected, result);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenAccountDoesNotExist()
	{
		Mock<IAccountRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.Account?)null);

		GetAccountByIdQueryHandler handler = new(mockRepository.Object);
		GetAccountByIdQuery query = new(Guid.NewGuid());
		Domain.Core.Account? result = await handler.Handle(query, CancellationToken.None);

		Assert.Null(result);
	}
}