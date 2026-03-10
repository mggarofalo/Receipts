using Domain.Core;
using Infrastructure.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Infrastructure.Mapping;

[Mapper]
public partial class CategoryMapper
{
	[MapperIgnoreTarget(nameof(CategoryEntity.Subcategories))]
	public partial CategoryEntity ToEntity(Category source);

	[MapperIgnoreSource(nameof(CategoryEntity.Subcategories))]
	public partial Category ToDomain(CategoryEntity source);
}
