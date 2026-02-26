using Application.Interfaces.Services;
using Application.Queries.Core.ItemTemplate;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Core.ItemTemplate;

public class GetDeletedItemTemplatesQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnDeletedItemTemplates()
	{
		List<Domain.Core.ItemTemplate> expected = ItemTemplateGenerator.GenerateList(2);

		Mock<IItemTemplateService> mockService = new();
		mockService.Setup(r => r.GetDeletedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetDeletedItemTemplatesQueryHandler handler = new(mockService.Object);
		GetDeletedItemTemplatesQuery query = new();

		List<Domain.Core.ItemTemplate> result = await handler.Handle(query, CancellationToken.None);

		result.Should().BeSameAs(expected);
	}
}
