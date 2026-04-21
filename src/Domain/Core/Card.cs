namespace Domain.Core;

public class Card
{
	public Guid Id { get; set; }
	public string CardCode { get; set; }
	public string Name { get; set; }
	public bool IsActive { get; set; }
	public Guid AccountId { get; set; }

	public const string CardCodeCannotBeEmpty = "Card code cannot be empty";
	public const string NameCannotBeEmpty = "Name cannot be empty";
	public const string AccountIdCannotBeEmpty = "Account ID cannot be empty";

	public Card(Guid id, string cardCode, string name, Guid accountId, bool isActive = true)
	{
		if (string.IsNullOrWhiteSpace(cardCode))
		{
			throw new ArgumentException(CardCodeCannotBeEmpty, nameof(cardCode));
		}

		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException(NameCannotBeEmpty, nameof(name));
		}

		if (accountId == Guid.Empty)
		{
			throw new ArgumentException(AccountIdCannotBeEmpty, nameof(accountId));
		}

		Id = id;
		CardCode = cardCode;
		Name = name;
		AccountId = accountId;
		IsActive = isActive;
	}
}
