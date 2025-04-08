using Common;

namespace Infrastructure.Entities.Core;

public class TransactionEntity : IEquatable<TransactionEntity>
{
	public Guid Id { get; set; }
	public Guid ReceiptId { get; set; }
	public Guid AccountId { get; set; }
	public decimal Amount { get; set; }
	public Currency AmountCurrency { get; set; }
	public DateOnly Date { get; set; }
	public virtual ReceiptEntity? Receipt { get; set; }
	public virtual AccountEntity? Account { get; set; }

	public bool Equals(TransactionEntity? other)
	{
		if (other is null)
		{
			return false;
		}

		return ReceiptId == other.ReceiptId &&
			   AccountId == other.AccountId &&
			   Amount == other.Amount &&
			   AmountCurrency == other.AmountCurrency &&
			   Date == other.Date;
	}

	public override bool Equals(object? obj)
	{
		if (obj is null)
		{
			return false;
		}

		if (obj.GetType() != GetType())
		{
			return false;
		}

		return Equals((TransactionEntity)obj);
	}

	public override int GetHashCode()
	{
		HashCode hash = new();
		hash.Add(Id);
		hash.Add(ReceiptId);
		hash.Add(AccountId);
		hash.Add(Amount);
		hash.Add(AmountCurrency);
		hash.Add(Date);
		return hash.ToHashCode();
	}

	public static bool operator ==(TransactionEntity? left, TransactionEntity? right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(TransactionEntity? left, TransactionEntity? right)
	{
		return !Equals(left, right);
	}
}