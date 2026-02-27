using Application.Interfaces.Services;

namespace Infrastructure.Services;

public class NullCurrentUserAccessor : ICurrentUserAccessor
{
	public string? UserId => null;
	public Guid? ApiKeyId => null;
	public string? IpAddress => null;
	public string? UserAgent => null;
}
