using Application.Commands.Subcategory.Delete;
using Application.Interfaces.Services;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Subcategory;

public class DeleteSubcategoryCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsTrueAndCallsDelete()
	{
		Mock<ISubcategoryService> mockService = new();
		DeleteSubcategoryCommandHandler handler = new(mockService.Object);

		List<Guid> input = [.. SubcategoryGenerator.GenerateList(2).Select(a => a.Id)];

		DeleteSubcategoryCommand command = new(input);
		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);
	}
}
