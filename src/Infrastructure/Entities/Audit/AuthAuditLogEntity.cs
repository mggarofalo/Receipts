namespace Infrastructure.Entities.Audit;

public class AuthAuditLogEntity
{
	public Guid Id { get; set; }
	public AuthEventType EventType { get; set; }
	public string? UserId { get; set; }
	public Guid? ApiKeyId { get; set; }
	public string? Username { get; set; }
	public bool Success { get; set; }
	public string? FailureReason { get; set; }
	public string? IpAddress { get; set; }
	public string? UserAgent { get; set; }
	public DateTimeOffset Timestamp { get; set; }
	public string? MetadataJson { get; set; }
}
