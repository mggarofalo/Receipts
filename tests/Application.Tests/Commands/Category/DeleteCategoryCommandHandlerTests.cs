using Application.Commands.Category.Delete;
using Application.Interfaces.Services;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Category;

public class DeleteCategoryCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsTrueAndCallsDelete()
	{
		Mock<ICategoryService> mockService = new();
		DeleteCategoryCommandHandler handler = new(mockService.Object);

		List<Guid> input = [.. CategoryGenerator.GenerateList(2).Select(a => a.Id)];

		DeleteCategoryCommand command = new(input);
		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);
	}
}
