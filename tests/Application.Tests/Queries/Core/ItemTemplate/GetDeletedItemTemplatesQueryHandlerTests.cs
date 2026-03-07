using Application.Interfaces.Services;
using Application.Models;
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
		mockService.Setup(r => r.GetDeletedAsync(0, 50, It.IsAny<SortParams>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PagedResult<Domain.Core.ItemTemplate>(expected, expected.Count, 0, 50));

		GetDeletedItemTemplatesQueryHandler handler = new(mockService.Object);
		GetDeletedItemTemplatesQuery query = new(0, 50, SortParams.Default);

		PagedResult<Domain.Core.ItemTemplate> result = await handler.Handle(query, CancellationToken.None);

		result.Data.Should().BeSameAs(expected);
	}
}
