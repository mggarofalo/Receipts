using Application.Interfaces.Services;

namespace Infrastructure.Tests.Helpers;

public class MockCurrentUserAccessor : ICurrentUserAccessor
{
	public string? UserId { get; set; }
	public Guid? ApiKeyId { get; set; }
	public string? IpAddress { get; set; }
	public string? UserAgent { get; set; }
}
