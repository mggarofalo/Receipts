using Application.Interfaces.Services;
using Application.Queries.Core.Category;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Core.Category;

public class GetCategoryByIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnCategory_WhenCategoryExists()
	{
		Domain.Core.Category expected = CategoryGenerator.Generate();

		Mock<ICategoryService> mockRService = new();
		mockRService.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetCategoryByIdQueryHandler handler = new(mockRService.Object);
		GetCategoryByIdQuery query = new(expected.Id);
		Domain.Core.Category? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		result.Should().BeSameAs(expected);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenCategoryDoesNotExist()
	{
		Mock<ICategoryService> mockRService = new();
		mockRService.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.Category?)null);

		GetCategoryByIdQueryHandler handler = new(mockRService.Object);
		GetCategoryByIdQuery query = new(Guid.NewGuid());
		Domain.Core.Category? result = await handler.Handle(query, CancellationToken.None);

		Assert.Null(result);
	}
}
