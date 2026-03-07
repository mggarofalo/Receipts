using Application.Interfaces.Services;
using Application.Models;
using Application.Queries.Core.ItemTemplate;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Core.ItemTemplate;

public class GetAllItemTemplatesQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnAllItemTemplates()
	{
		List<Domain.Core.ItemTemplate> expected = ItemTemplateGenerator.GenerateList(2);

		Mock<IItemTemplateService> mockService = new();
		mockService.Setup(r => r.GetAllAsync(0, 50, It.IsAny<SortParams>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PagedResult<Domain.Core.ItemTemplate>(expected, expected.Count, 0, 50));

		GetAllItemTemplatesQueryHandler handler = new(mockService.Object);
		GetAllItemTemplatesQuery query = new(0, 50, SortParams.Default);

		PagedResult<Domain.Core.ItemTemplate> result = await handler.Handle(query, CancellationToken.None);

		result.Data.Should().BeSameAs(expected);
	}
}
