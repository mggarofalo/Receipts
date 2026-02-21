namespace Application.Interfaces.Services;

public record AuditLogDto(
	Guid Id,
	string EntityType,
	string EntityId,
	string Action,
	string ChangesJson,
	string? ChangedByUserId,
	Guid? ChangedByApiKeyId,
	DateTimeOffset ChangedAt,
	string? IpAddress);
