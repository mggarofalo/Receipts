using Application.Interfaces.Services;
using Application.Queries.Core.Category;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Core.Category;

public class GetAllCategoriesQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnAllCategories()
	{
		List<Domain.Core.Category> expected = CategoryGenerator.GenerateList(2);

		Mock<ICategoryService> mockService = new();
		mockService.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetAllCategoriesQueryHandler handler = new(mockService.Object);
		GetAllCategoriesQuery query = new();

		List<Domain.Core.Category> result = await handler.Handle(query, CancellationToken.None);

		result.Should().BeSameAs(expected);
	}
}
