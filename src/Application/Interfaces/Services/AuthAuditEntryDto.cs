namespace Application.Interfaces.Services;

public record AuthAuditEntryDto(
	Guid Id,
	string EventType,
	string? UserId,
	Guid? ApiKeyId,
	string? Username,
	bool Success,
	string? FailureReason,
	string? IpAddress,
	string? UserAgent,
	DateTimeOffset Timestamp,
	string? MetadataJson);
