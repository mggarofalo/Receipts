using Application.Interfaces.Services;
using Application.Queries.Core.Card;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Core.Card;

public class GetCardsByAccountIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnCards_WhenAccountHasCards()
	{
		Guid accountId = Guid.NewGuid();
		List<Domain.Core.Card> expected = CardGenerator.GenerateList(3);

		Mock<ICardService> mockService = new();
		mockService.Setup(s => s.GetByAccountIdAsync(accountId, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetCardsByAccountIdQueryHandler handler = new(mockService.Object);
		GetCardsByAccountIdQuery query = new(accountId);
		List<Domain.Core.Card> result = await handler.Handle(query, CancellationToken.None);

		result.Should().BeEquivalentTo(expected);
	}

	[Fact]
	public async Task Handle_ShouldReturnEmptyList_WhenAccountHasNoCards()
	{
		Guid accountId = Guid.NewGuid();

		Mock<ICardService> mockService = new();
		mockService.Setup(s => s.GetByAccountIdAsync(accountId, It.IsAny<CancellationToken>())).ReturnsAsync([]);

		GetCardsByAccountIdQueryHandler handler = new(mockService.Object);
		GetCardsByAccountIdQuery query = new(accountId);
		List<Domain.Core.Card> result = await handler.Handle(query, CancellationToken.None);

		result.Should().BeEmpty();
	}
}
