using Application.Interfaces.Services;
using Application.Queries.Core.Subcategory;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Core.Subcategory;

public class GetAllSubcategoriesQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnAllSubcategories()
	{
		List<Domain.Core.Subcategory> expected = SubcategoryGenerator.GenerateList(2);

		Mock<ISubcategoryService> mockService = new();
		mockService.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetAllSubcategoriesQueryHandler handler = new(mockService.Object);
		GetAllSubcategoriesQuery query = new();

		List<Domain.Core.Subcategory> result = await handler.Handle(query, CancellationToken.None);

		result.Should().BeSameAs(expected);
	}
}
