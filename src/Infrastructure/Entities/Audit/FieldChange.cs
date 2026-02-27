namespace Infrastructure.Entities.Audit;

public class FieldChange
{
	public required string FieldName { get; set; }
	public string? OldValue { get; set; }
	public string? NewValue { get; set; }
}
