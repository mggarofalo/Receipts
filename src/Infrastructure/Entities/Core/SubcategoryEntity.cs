namespace Infrastructure.Entities.Core;

public class SubcategoryEntity
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public Guid CategoryId { get; set; }
	public string? Description { get; set; }
	public virtual CategoryEntity? Category { get; set; }
}
