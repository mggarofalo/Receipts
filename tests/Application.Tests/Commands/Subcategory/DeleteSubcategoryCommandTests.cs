using Application.Commands.Subcategory.Delete;
using FluentAssertions;

namespace Application.Tests.Commands.Subcategory;

public class DeleteSubcategoryCommandTests
{
	[Fact]
	public void Command_WithEmptyId_ThrowsArgumentException()
	{
		Assert.Throws<ArgumentException>(() => new DeleteSubcategoryCommand(Guid.Empty));
	}

	[Fact]
	public void Command_WithValidId_ReturnsValidCommand()
	{
		Guid id = Guid.NewGuid();
		DeleteSubcategoryCommand command = new(id);
		command.Id.Should().Be(id);
	}
}
