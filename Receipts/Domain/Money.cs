using Common;

namespace Domain;

public record Money(decimal Amount, Currency Currency = Currency.USD);
