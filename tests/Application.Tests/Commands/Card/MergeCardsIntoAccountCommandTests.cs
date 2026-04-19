using Application.Commands.Card.Merge;
using FluentAssertions;

namespace Application.Tests.Commands.Card;

public class MergeCardsIntoAccountCommandTests
{
	[Fact]
	public void Constructor_WithEmptyTargetId_Throws()
	{
		Action act = () => _ = new MergeCardsIntoAccountCommand(Guid.Empty, [Guid.NewGuid(), Guid.NewGuid()]);
		act.Should().Throw<ArgumentException>()
			.WithMessage(MergeCardsIntoAccountCommand.TargetIdCannotBeEmpty + "*");
	}

	[Fact]
	public void Constructor_WithEmptySourceCardIds_Throws()
	{
		Action act = () => _ = new MergeCardsIntoAccountCommand(Guid.NewGuid(), []);
		act.Should().Throw<ArgumentException>()
			.WithMessage(MergeCardsIntoAccountCommand.SourceCardIdsCannotBeEmpty + "*");
	}

	[Fact]
	public void Constructor_WithValidArgs_SetsProperties()
	{
		Guid target = Guid.NewGuid();
		Guid winner = Guid.NewGuid();
		Guid c1 = Guid.NewGuid();
		Guid c2 = Guid.NewGuid();

		MergeCardsIntoAccountCommand command = new(target, [c1, c2], winner);

		command.TargetAccountId.Should().Be(target);
		command.SourceCardIds.Should().Equal(c1, c2);
		command.YnabMappingWinnerAccountId.Should().Be(winner);
	}
}
