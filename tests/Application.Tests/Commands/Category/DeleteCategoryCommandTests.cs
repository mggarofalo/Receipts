using Application.Commands.Category.Delete;
using FluentAssertions;

namespace Application.Tests.Commands.Category;

public class DeleteCategoryCommandTests
{
	[Fact]
	public void Command_WithEmptyId_ThrowsArgumentException()
	{
		Assert.Throws<ArgumentException>(() => new DeleteCategoryCommand(Guid.Empty));
	}

	[Fact]
	public void Command_WithValidId_ReturnsValidCommand()
	{
		Guid id = Guid.NewGuid();
		DeleteCategoryCommand command = new(id);
		command.Id.Should().Be(id);
	}
}
