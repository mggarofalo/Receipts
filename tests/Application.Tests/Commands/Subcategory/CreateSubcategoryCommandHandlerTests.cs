using Application.Commands.Subcategory.Create;
using Application.Interfaces.Services;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Subcategory;

public class CreateSubcategoryCommandHandlerTests
{
	[Fact]
	public async Task CreateSubcategoryCommandHandler_WithValidCommand_ReturnsCreatedSubcategories()
	{
		Mock<ISubcategoryService> mockService = new();
		CreateSubcategoryCommandHandler handler = new(mockService.Object);

		List<Domain.Core.Subcategory> input = SubcategoryGenerator.GenerateList(1);

		mockService.Setup(r => r
			.CreateAsync(It.IsAny<List<Domain.Core.Subcategory>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(input);

		CreateSubcategoryCommand command = new(input);
		List<Domain.Core.Subcategory> result = await handler.Handle(command, CancellationToken.None);

		result.Should().HaveCount(input.Count);
	}
}
