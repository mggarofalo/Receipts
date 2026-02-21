namespace Infrastructure.Entities.Audit;

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
}
