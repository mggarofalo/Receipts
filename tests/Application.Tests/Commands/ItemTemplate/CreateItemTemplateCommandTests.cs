using Application.Commands.ItemTemplate.Create;
using FluentAssertions;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.ItemTemplate;

public class CreateItemTemplateCommandTests : ICommandTests<Domain.Core.ItemTemplate>
{
	[Fact]
	public void Command_WithNullItems_ThrowsArgumentNullException()
	{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
		List<Domain.Core.ItemTemplate> items = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
		Assert.Throws<ArgumentNullException>(() => new CreateItemTemplateCommand(items));
#pragma warning restore CS8604 // Possible null reference argument.
	}

	[Fact]
	public void Command_WithEmptyItems_ThrowsArgumentException()
	{
		List<Domain.Core.ItemTemplate> items = [];
		Assert.Throws<ArgumentException>(() => new CreateItemTemplateCommand(items));
	}

	[Fact]
	public void Command_WithValidItems_ReturnsValidCommand()
	{
		List<Domain.Core.ItemTemplate> items = ItemTemplateGenerator.GenerateList(2);
		CreateItemTemplateCommand command = new(items);
		command.ItemTemplates.Should().BeEquivalentTo(items);
	}

	[Fact]
	public void Items_ShouldBeImmutable()
	{
		List<Domain.Core.ItemTemplate> items = ItemTemplateGenerator.GenerateList(2);
		CreateItemTemplateCommand command = new(items);
		Assert.IsAssignableFrom<IReadOnlyList<Domain.Core.ItemTemplate>>(command.ItemTemplates);
		Assert.NotSame(items, command.ItemTemplates);
	}
}
