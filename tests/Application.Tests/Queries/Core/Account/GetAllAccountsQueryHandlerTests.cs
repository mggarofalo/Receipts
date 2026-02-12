using Application.Interfaces.Services;
using Application.Queries.Core.Account;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Core.Account;

public class GetAllAccountsQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnAllAccounts()
	{
		List<Domain.Core.Account> expected = AccountGenerator.GenerateList(2);

		Mock<IAccountService> mockService = new();
		mockService.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetAllAccountsQueryHandler handler = new(mockService.Object);
		GetAllAccountsQuery query = new();

		List<Domain.Core.Account> result = await handler.Handle(query, CancellationToken.None);

		result.Should().BeSameAs(expected);
	}
}