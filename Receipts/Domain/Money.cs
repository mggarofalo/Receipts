using Common;

namespace Domain;

public record Money(decimal Amount, Currency Currency)
{
	public Money(decimal amount) : this(amount, Currency.USD) { }

	public static Money Zero => new(0);

	public static Money operator +(Money a, Money b)
	{
		return new Money(a.Amount + b.Amount, a.Currency);
	}

	public static Money operator -(Money a, Money b)
	{
		return new Money(a.Amount - b.Amount, a.Currency);
	}

	public static Money operator *(Money a, Money b)
	{
		return new Money(Math.Round(a.Amount * b.Amount, 2), a.Currency);
	}

	public static Money operator /(Money a, Money b)
	{
		return new Money(Math.Round(a.Amount / b.Amount, 2), a.Currency);
	}
}
