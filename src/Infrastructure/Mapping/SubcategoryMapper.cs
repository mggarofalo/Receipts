using Domain.Core;
using Infrastructure.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Infrastructure.Mapping;

[Mapper]
public partial class SubcategoryMapper
{
	[MapperIgnoreTarget(nameof(SubcategoryEntity.Category))]
	public partial SubcategoryEntity ToEntity(Subcategory source);

	[MapperIgnoreSource(nameof(SubcategoryEntity.Category))]
	public partial Subcategory ToDomain(SubcategoryEntity source);
}
