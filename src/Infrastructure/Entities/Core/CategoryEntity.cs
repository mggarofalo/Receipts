namespace Infrastructure.Entities.Core;

public class CategoryEntity
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	public virtual ICollection<SubcategoryEntity> Subcategories { get; set; } = [];
}
