using Infrastructure.Entities.Audit;

namespace Infrastructure.Tests.Entities.Audit;

public class AuditLogEntityTests
{
	[Fact]
	public void GetChanges_DefaultChangesJson_ReturnsEmptyList()
	{
		// Arrange — default ChangesJson is "[]"
		AuditLogEntity entity = new()
		{
			EntityType = "Receipt",
			EntityId = Guid.NewGuid().ToString(),
		};

		// Act
		List<FieldChange> changes = entity.GetChanges();

		// Assert
		Assert.Empty(changes);
	}

	[Fact]
	public void GetChanges_NullChangesJson_ReturnsEmptyList()
	{
		// Arrange
		AuditLogEntity entity = new()
		{
			EntityType = "Receipt",
			EntityId = Guid.NewGuid().ToString(),
			ChangesJson = null!,
		};

		// Act
		List<FieldChange> changes = entity.GetChanges();

		// Assert
		Assert.Empty(changes);
	}

	[Fact]
	public void GetChanges_EmptyStringChangesJson_ReturnsEmptyList()
	{
		// Arrange
		AuditLogEntity entity = new()
		{
			EntityType = "Receipt",
			EntityId = Guid.NewGuid().ToString(),
			ChangesJson = "",
		};

		// Act
		List<FieldChange> changes = entity.GetChanges();

		// Assert
		Assert.Empty(changes);
	}

	[Fact]
	public void GetChanges_ValidChangesJson_ReturnsDeserializedList()
	{
		// Arrange
		AuditLogEntity entity = new()
		{
			EntityType = "Receipt",
			EntityId = Guid.NewGuid().ToString(),
			ChangesJson = """[{"FieldName":"Name","OldValue":"Old","NewValue":"New"}]""",
		};

		// Act
		List<FieldChange> changes = entity.GetChanges();

		// Assert
		Assert.Single(changes);
		Assert.Equal("Name", changes[0].FieldName);
		Assert.Equal("Old", changes[0].OldValue);
		Assert.Equal("New", changes[0].NewValue);
	}

	[Fact]
	public void GetChanges_MultipleChanges_ReturnsAllEntries()
	{
		// Arrange
		AuditLogEntity entity = new()
		{
			EntityType = "Account",
			EntityId = Guid.NewGuid().ToString(),
			ChangesJson = """[{"FieldName":"Name","OldValue":"A","NewValue":"B"},{"FieldName":"Code","OldValue":null,"NewValue":"C"}]""",
		};

		// Act
		List<FieldChange> changes = entity.GetChanges();

		// Assert
		Assert.Equal(2, changes.Count);
		Assert.Equal("Name", changes[0].FieldName);
		Assert.Equal("Code", changes[1].FieldName);
		Assert.Null(changes[1].OldValue);
	}

	[Fact]
	public void SetChanges_SerializesToJson()
	{
		// Arrange
		AuditLogEntity entity = new()
		{
			EntityType = "Receipt",
			EntityId = Guid.NewGuid().ToString(),
		};
		List<FieldChange> changes =
		[
			new() { FieldName = "Amount", OldValue = "10.00", NewValue = "20.00" },
		];

		// Act
		entity.SetChanges(changes);

		// Assert
		Assert.Contains("Amount", entity.ChangesJson);
		Assert.Contains("10.00", entity.ChangesJson);
		Assert.Contains("20.00", entity.ChangesJson);
	}

	[Fact]
	public void SetChanges_ThenGetChanges_RoundTrips()
	{
		// Arrange
		AuditLogEntity entity = new()
		{
			EntityType = "Receipt",
			EntityId = Guid.NewGuid().ToString(),
		};
		List<FieldChange> original =
		[
			new() { FieldName = "Description", OldValue = "Old Desc", NewValue = "New Desc" },
			new() { FieldName = "Category", OldValue = null, NewValue = "Groceries" },
		];

		// Act
		entity.SetChanges(original);
		List<FieldChange> roundTripped = entity.GetChanges();

		// Assert
		Assert.Equal(original.Count, roundTripped.Count);
		Assert.Equal(original[0].FieldName, roundTripped[0].FieldName);
		Assert.Equal(original[0].OldValue, roundTripped[0].OldValue);
		Assert.Equal(original[0].NewValue, roundTripped[0].NewValue);
		Assert.Equal(original[1].FieldName, roundTripped[1].FieldName);
		Assert.Null(roundTripped[1].OldValue);
		Assert.Equal(original[1].NewValue, roundTripped[1].NewValue);
	}
}
