using System.Text.Json;

namespace Infrastructure.Entities.Audit;

public class AuditLogEntity
{
	public Guid Id { get; set; }
	public required string EntityType { get; set; }
	public required string EntityId { get; set; }
	public AuditAction Action { get; set; }
	public string ChangesJson { get; set; } = "[]";
	public string? ChangedByUserId { get; set; }
	public Guid? ChangedByApiKeyId { get; set; }
	public DateTimeOffset ChangedAt { get; set; }
	public string? IpAddress { get; set; }

	public List<FieldChange> GetChanges()
	{
		if (string.IsNullOrEmpty(ChangesJson))
		{
			return [];
		}

		return JsonSerializer.Deserialize<List<FieldChange>>(ChangesJson) ?? [];
	}

	public void SetChanges(List<FieldChange> changes)
	{
		ChangesJson = JsonSerializer.Serialize(changes);
	}
}
