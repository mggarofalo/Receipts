using Application.Commands.Category.Create;
using Application.Interfaces.Services;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Category;

public class CreateCategoryCommandHandlerTests
{
	[Fact]
	public async Task CreateCategoryCommandHandler_WithValidCommand_ReturnsCreatedCategories()
	{
		Mock<ICategoryService> mockService = new();
		CreateCategoryCommandHandler handler = new(mockService.Object);

		List<Domain.Core.Category> input = CategoryGenerator.GenerateList(1);

		mockService.Setup(r => r
			.CreateAsync(It.IsAny<List<Domain.Core.Category>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(input);

		CreateCategoryCommand command = new(input);
		List<Domain.Core.Category> result = await handler.Handle(command, CancellationToken.None);

		result.Should().HaveCount(input.Count);
	}
}
