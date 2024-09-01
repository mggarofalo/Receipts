using Moq;
using Application.Queries.Account;
using Application.Interfaces.Repositories;

namespace Application.Tests.Queries.Account;

public class GetAccountByIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnAccount_WhenAccountExists()
	{
		Guid accountId = Guid.NewGuid();
		Domain.Core.Account account = new(accountId, "ACCT_1", "Test Account 1", true);

		Mock<IAccountRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(account);

		GetAccountByIdQueryHandler handler = new(mockRepository.Object);
		GetAccountByIdQuery query = new(accountId);

		Domain.Core.Account? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
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
		mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
	}
}