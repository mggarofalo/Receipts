using Application.Interfaces.Services;
using Application.Queries.Core.ItemTemplate;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Core.ItemTemplate;

public class GetItemTemplateByIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnItemTemplate_WhenItemTemplateExists()
	{
		Domain.Core.ItemTemplate expected = ItemTemplateGenerator.Generate();

		Mock<IItemTemplateService> mockService = new();
		mockService.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetItemTemplateByIdQueryHandler handler = new(mockService.Object);
		GetItemTemplateByIdQuery query = new(expected.Id);
		Domain.Core.ItemTemplate? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		result.Should().BeSameAs(expected);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenItemTemplateDoesNotExist()
	{
		Mock<IItemTemplateService> mockService = new();
		mockService.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.ItemTemplate?)null);

		GetItemTemplateByIdQueryHandler handler = new(mockService.Object);
		GetItemTemplateByIdQuery query = new(Guid.NewGuid());
		Domain.Core.ItemTemplate? result = await handler.Handle(query, CancellationToken.None);

		Assert.Null(result);
	}
}
