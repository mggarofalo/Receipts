using Domain.Core;
using Infrastructure.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Infrastructure.Mapping;

[Mapper]
public partial class CategoryMapper
{
	[MapperIgnoreTarget(nameof(CategoryEntity.DeletedAt))]
	[MapperIgnoreTarget(nameof(CategoryEntity.DeletedByUserId))]
	[MapperIgnoreTarget(nameof(CategoryEntity.DeletedByApiKeyId))]
	public partial CategoryEntity ToEntity(Category source);

	[MapperIgnoreSource(nameof(CategoryEntity.DeletedAt))]
	[MapperIgnoreSource(nameof(CategoryEntity.DeletedByUserId))]
	[MapperIgnoreSource(nameof(CategoryEntity.DeletedByApiKeyId))]
	public partial Category ToDomain(CategoryEntity source);
}
