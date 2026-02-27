using Infrastructure.Entities.Audit;

namespace SampleData.Entities;

public static class AuditLogEntityGenerator
{
	public static AuditLogEntity Generate(
		AuditAction action = AuditAction.Create,
		string entityType = "Account",
		string? entityId = null,
		string? changedByUserId = null,
		Guid? changedByApiKeyId = null,
		string? ipAddress = null)
	{
		AuditLogEntity entity = new()
		{
			Id = Guid.NewGuid(),
			EntityType = entityType,
			EntityId = entityId ?? Guid.NewGuid().ToString(),
			Action = action,
			ChangedByUserId = changedByUserId,
			ChangedByApiKeyId = changedByApiKeyId,
			ChangedAt = DateTimeOffset.UtcNow,
			IpAddress = ipAddress,
		};

		entity.SetChanges([
			new FieldChange
			{
				FieldName = "Name",
				OldValue = null,
				NewValue = "Test Value",
			}
		]);

		return entity;
	}

	public static List<AuditLogEntity> GenerateList(int count)
	{
		return [.. Enumerable.Range(0, count).Select(_ => Generate())];
	}
}
