using Application.Commands.Card.Create;
using FluentAssertions;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Card;

public class CreateCardCommandTests : ICommandTests<Domain.Core.Card>
{
	[Fact]
	public void Command_WithNullItems_ThrowsArgumentNullException()
	{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
		List<Domain.Core.Card> items = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
		Assert.Throws<ArgumentNullException>(() => new CreateCardCommand(items));
#pragma warning restore CS8604 // Possible null reference argument.
	}

	[Fact]
	public void Command_WithEmptyItems_ThrowsArgumentException()
	{
		List<Domain.Core.Card> items = [];
		Assert.Throws<ArgumentException>(() => new CreateCardCommand(items));
	}

	[Fact]
	public void Command_WithValidItems_ReturnsValidCommand()
	{
		List<Domain.Core.Card> items = CardGenerator.GenerateList(2);
		CreateCardCommand command = new(items);
		command.Cards.Should().BeEquivalentTo(items);
	}

	[Fact]
	public void Items_ShouldBeImmutable()
	{
		List<Domain.Core.Card> items = CardGenerator.GenerateList(2);
		CreateCardCommand command = new(items);
		Assert.IsAssignableFrom<IReadOnlyList<Domain.Core.Card>>(command.Cards);
		Assert.NotSame(items, command.Cards);
	}
}
