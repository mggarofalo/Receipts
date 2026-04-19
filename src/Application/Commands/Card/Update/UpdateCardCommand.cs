using Application.Interfaces;

namespace Application.Commands.Card.Update;

public record UpdateCardCommand : ICommand<bool>
{
	public IReadOnlyList<Domain.Core.Card> Cards { get; }

	public const string CardsListCannotBeEmpty = "Cards list cannot be empty.";

	public UpdateCardCommand(List<Domain.Core.Card> cards)
	{
		ArgumentNullException.ThrowIfNull(cards);

		if (cards.Count == 0)
		{
			throw new ArgumentException(CardsListCannotBeEmpty, nameof(cards));
		}

		Cards = cards.AsReadOnly();
	}
}
