namespace Application.Interfaces.Services;

public interface ICurrentUserAccessor
{
	string? UserId { get; }
	Guid? ApiKeyId { get; }
	string? IpAddress { get; }
	string? UserAgent { get; }
}
