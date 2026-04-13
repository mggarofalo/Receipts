using Application.Interfaces.Services;
using Application.Queries.Core.Card;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Core.Card;

public class GetCardByIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnAccount_WhenAccountExists()
	{
		Domain.Core.Card expected = CardGenerator.Generate();

		Mock<ICardService> mockRService = new();
		mockRService.Setup(r => r.GetByIdAsync(expected.Id, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetCardByIdQueryHandler handler = new(mockRService.Object);
		GetCardByIdQuery query = new(expected.Id);
		Domain.Core.Card? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		result.Should().BeSameAs(expected);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenAccountDoesNotExist()
	{
		Guid missingId = Guid.NewGuid();
		Mock<ICardService> mockRService = new();
		mockRService.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.Card?)null);

		GetCardByIdQueryHandler handler = new(mockRService.Object);
		GetCardByIdQuery query = new(missingId);
		Domain.Core.Card? result = await handler.Handle(query, CancellationToken.None);

		Assert.Null(result);
	}
}