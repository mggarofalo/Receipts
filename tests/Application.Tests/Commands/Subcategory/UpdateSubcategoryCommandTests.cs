using Application.Commands.Subcategory.Update;
using FluentAssertions;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Subcategory;

public class UpdateSubcategoryCommandTests : ICommandTests<Domain.Core.Subcategory>
{
	[Fact]
	public void Command_WithNullItems_ThrowsArgumentNullException()
	{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
		List<Domain.Core.Subcategory> items = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
		Assert.Throws<ArgumentNullException>(() => new UpdateSubcategoryCommand(items));
#pragma warning restore CS8604 // Possible null reference argument.
	}

	[Fact]
	public void Command_WithEmptyItems_ThrowsArgumentException()
	{
		List<Domain.Core.Subcategory> items = [];
		Assert.Throws<ArgumentException>(() => new UpdateSubcategoryCommand(items));
	}

	[Fact]
	public void Command_WithValidItems_ReturnsValidCommand()
	{
		List<Domain.Core.Subcategory> items = SubcategoryGenerator.GenerateList(2);
		UpdateSubcategoryCommand command = new(items);
		command.Subcategories.Should().BeEquivalentTo(items);
	}

	[Fact]
	public void Items_ShouldBeImmutable()
	{
		List<Domain.Core.Subcategory> items = SubcategoryGenerator.GenerateList(2);
		UpdateSubcategoryCommand command = new(items);
		Assert.IsAssignableFrom<IReadOnlyList<Domain.Core.Subcategory>>(command.Subcategories);
		Assert.NotSame(items, command.Subcategories);
	}
}
