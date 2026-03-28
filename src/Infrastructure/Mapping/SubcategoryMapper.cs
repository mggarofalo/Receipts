using Domain.Core;
using Infrastructure.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Infrastructure.Mapping;

[Mapper]
public partial class SubcategoryMapper
{
	[MapperIgnoreTarget(nameof(SubcategoryEntity.Category))]
	[MapperIgnoreTarget(nameof(SubcategoryEntity.DeletedAt))]
	[MapperIgnoreTarget(nameof(SubcategoryEntity.DeletedByUserId))]
	[MapperIgnoreTarget(nameof(SubcategoryEntity.DeletedByApiKeyId))]
	[MapperIgnoreTarget(nameof(SubcategoryEntity.CascadeDeletedByParentId))]
	public partial SubcategoryEntity ToEntity(Subcategory source);

	[MapperIgnoreSource(nameof(SubcategoryEntity.Category))]
	[MapperIgnoreSource(nameof(SubcategoryEntity.DeletedAt))]
	[MapperIgnoreSource(nameof(SubcategoryEntity.DeletedByUserId))]
	[MapperIgnoreSource(nameof(SubcategoryEntity.DeletedByApiKeyId))]
	[MapperIgnoreSource(nameof(SubcategoryEntity.CascadeDeletedByParentId))]
	public partial Subcategory ToDomain(SubcategoryEntity source);
}
