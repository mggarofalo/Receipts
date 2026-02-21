using Infrastructure.Entities.Audit;

namespace SampleData.Entities;

public static class AuthAuditLogEntityGenerator
{
	public static AuthAuditLogEntity Generate(
		AuthEventType eventType = AuthEventType.Login,
		string? userId = null,
		Guid? apiKeyId = null,
		string? username = null,
		bool success = true,
		string? failureReason = null,
		string? ipAddress = null,
		string? userAgent = null,
		DateTimeOffset? timestamp = null,
		string? metadataJson = null)
	{
		return new AuthAuditLogEntity
		{
			Id = Guid.NewGuid(),
			EventType = eventType,
			UserId = userId ?? "test-user-id",
			ApiKeyId = apiKeyId,
			Username = username ?? "testuser@example.com",
			Success = success,
			FailureReason = failureReason,
			IpAddress = ipAddress ?? "192.168.1.1",
			UserAgent = userAgent ?? "TestAgent/1.0",
			Timestamp = timestamp ?? DateTimeOffset.UtcNow,
			MetadataJson = metadataJson,
		};
	}

	public static List<AuthAuditLogEntity> GenerateList(int count)
	{
		return [.. Enumerable.Range(0, count).Select(_ => Generate())];
	}
}
