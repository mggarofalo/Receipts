using Application.Commands.ItemTemplate.Delete;
using Application.Interfaces.Services;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.ItemTemplate;

public class DeleteItemTemplateCommandHandlerTests
{
	[Fact]
	public async Task DeleteItemTemplateCommandHandler_WithValidCommand_ReturnsTrueAndCallsDeleteAndSaveChanges()
	{
		Mock<IItemTemplateService> mockService = new();
		DeleteItemTemplateCommandHandler handler = new(mockService.Object);

		List<Guid> input = [.. ItemTemplateGenerator.GenerateList(2).Select(a => a.Id)];

		DeleteItemTemplateCommand command = new(input);
		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);
	}
}
