using Application.Interfaces.Services;
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
		mockService.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetAllItemTemplatesQueryHandler handler = new(mockService.Object);
		GetAllItemTemplatesQuery query = new();

		List<Domain.Core.ItemTemplate> result = await handler.Handle(query, CancellationToken.None);

		result.Should().BeSameAs(expected);
	}
}
