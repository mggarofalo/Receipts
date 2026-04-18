using Application.Interfaces.Services;
using Application.Queries.Core.Account;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Core.Account;

public class GetAccountByIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnAccount_WhenAccountExists()
	{
		Domain.Core.Account expected = AccountGenerator.Generate();

		Mock<IAccountService> mockRService = new();
		mockRService.Setup(r => r.GetByIdAsync(expected.Id, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetAccountByIdQueryHandler handler = new(mockRService.Object);
		GetAccountByIdQuery query = new(expected.Id);
		Domain.Core.Account? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		result.Should().BeSameAs(expected);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenAccountDoesNotExist()
	{
		Guid missingId = Guid.NewGuid();
		Mock<IAccountService> mockRService = new();
		mockRService.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.Account?)null);

		GetAccountByIdQueryHandler handler = new(mockRService.Object);
		GetAccountByIdQuery query = new(missingId);
		Domain.Core.Account? result = await handler.Handle(query, CancellationToken.None);

		Assert.Null(result);
	}
}
