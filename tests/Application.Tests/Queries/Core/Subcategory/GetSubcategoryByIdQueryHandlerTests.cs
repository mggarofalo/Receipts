using Application.Interfaces.Services;
using Application.Queries.Core.Subcategory;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Core.Subcategory;

public class GetSubcategoryByIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnSubcategory_WhenSubcategoryExists()
	{
		Domain.Core.Subcategory expected = SubcategoryGenerator.Generate();

		Mock<ISubcategoryService> mockRService = new();
		mockRService.Setup(r => r.GetByIdAsync(expected.Id, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetSubcategoryByIdQueryHandler handler = new(mockRService.Object);
		GetSubcategoryByIdQuery query = new(expected.Id);
		Domain.Core.Subcategory? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		result.Should().BeSameAs(expected);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenSubcategoryDoesNotExist()
	{
		Guid missingId = Guid.NewGuid();
		Mock<ISubcategoryService> mockRService = new();
		mockRService.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.Subcategory?)null);

		GetSubcategoryByIdQueryHandler handler = new(mockRService.Object);
		GetSubcategoryByIdQuery query = new(missingId);
		Domain.Core.Subcategory? result = await handler.Handle(query, CancellationToken.None);

		Assert.Null(result);
	}
}
