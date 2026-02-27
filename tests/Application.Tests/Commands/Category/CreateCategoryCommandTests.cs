using Application.Commands.Category.Create;
using FluentAssertions;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Category;

public class CreateCategoryCommandTests : ICommandTests<Domain.Core.Category>
{
	[Fact]
	public void Command_WithNullItems_ThrowsArgumentNullException()
	{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
		List<Domain.Core.Category> items = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
		Assert.Throws<ArgumentNullException>(() => new CreateCategoryCommand(items));
#pragma warning restore CS8604 // Possible null reference argument.
	}

	[Fact]
	public void Command_WithEmptyItems_ThrowsArgumentException()
	{
		List<Domain.Core.Category> items = [];
		Assert.Throws<ArgumentException>(() => new CreateCategoryCommand(items));
	}

	[Fact]
	public void Command_WithValidItems_ReturnsValidCommand()
	{
		List<Domain.Core.Category> items = CategoryGenerator.GenerateList(2);
		CreateCategoryCommand command = new(items);
		command.Categories.Should().BeEquivalentTo(items);
	}

	[Fact]
	public void Items_ShouldBeImmutable()
	{
		List<Domain.Core.Category> items = CategoryGenerator.GenerateList(2);
		CreateCategoryCommand command = new(items);
		Assert.IsAssignableFrom<IReadOnlyList<Domain.Core.Category>>(command.Categories);
		Assert.NotSame(items, command.Categories);
	}
}
