using Application.Interfaces.Repositories;
using SampleData.Domain.Core;
using Moq;
using Application.Queries.Core.Account;

namespace Application.Tests.Queries.Core.Account;

public class GetAllAccountsQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnAllAccounts()
	{
		List<Domain.Core.Account> accounts = AccountGenerator.GenerateList(2);

		Mock<IAccountRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(accounts);

		GetAllAccountsQueryHandler handler = new(mockRepository.Object);
		GetAllAccountsQuery query = new();

		List<Domain.Core.Account> result = await handler.Handle(query, CancellationToken.None);

		Assert.Equal(accounts.Count, result.Count);
		Assert.True(accounts.All(input => result.Any(output =>
			output.Id == input.Id &&
			output.AccountCode == input.AccountCode &&
			output.Name == input.Name &&
			output.IsActive == input.IsActive)));

		mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}