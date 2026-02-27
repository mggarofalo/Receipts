using Application.Commands.ItemTemplate.Create;
using Application.Interfaces.Services;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.ItemTemplate;

public class CreateItemTemplateCommandHandlerTests
{
	[Fact]
	public async Task CreateItemTemplateCommandHandler_WithValidCommand_ReturnsCreatedItemTemplates()
	{
		Mock<IItemTemplateService> mockService = new();
		CreateItemTemplateCommandHandler handler = new(mockService.Object);

		List<Domain.Core.ItemTemplate> input = ItemTemplateGenerator.GenerateList(1);

		mockService.Setup(r => r
			.CreateAsync(It.IsAny<List<Domain.Core.ItemTemplate>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(input);

		CreateItemTemplateCommand command = new(input);
		List<Domain.Core.ItemTemplate> result = await handler.Handle(command, CancellationToken.None);

		result.Should().HaveCount(input.Count);
	}
}
