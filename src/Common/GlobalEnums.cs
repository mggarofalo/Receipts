namespace Common;

public enum Currency
{
	USD
}

public enum PricingMode
{
	Quantity,
	Flat
}

public enum AuthEventType
{
	Login,
	LoginFailed,
	Logout,
	ApiKeyUsed,
	ApiKeyCreated,
	ApiKeyRevoked,
	PasswordChanged,
	UserRegistered,
	AccountDisabled,
}
