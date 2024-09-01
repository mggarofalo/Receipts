using Application.Queries.Account;
using Application.Interfaces.Repositories;
using Domain;
using Moq;

namespace Application.Tests.Queries.Account;

public class GetAllAccountsQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnAllAccounts()
	{
		List<Domain.Core.Account> allAccounts =
		[
			new(Guid.NewGuid(), "ACCT_1", "Test Account 1", true),
			new(Guid.NewGuid(), "ACCT_2", "Test Account 2", true)
		];

		Mock<IAccountRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(allAccounts);

		GetAllAccountsQueryHandler handler = new(mockRepository.Object);
		GetAllAccountsQuery query = new();

		List<Domain.Core.Account> result = await handler.Handle(query, CancellationToken.None);

		Assert.Equal(allAccounts.Count, result.Count);
		Assert.True(allAccounts.All(a => result.Any(ra =>
		ra.Id == a.Id &&
			ra.AccountCode == a.AccountCode &&
			ra.Name == a.Name &&
			ra.IsActive == a.IsActive)));

		mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}