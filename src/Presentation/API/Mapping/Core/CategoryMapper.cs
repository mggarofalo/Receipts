using API.Generated.Dtos;
using Domain.Core;
using Riok.Mapperly.Abstractions;

namespace API.Mapping.Core;

[Mapper]
public partial class CategoryMapper
{
	[MapperIgnoreTarget(nameof(CategoryResponse.AdditionalProperties))]
	public partial CategoryResponse ToResponse(Category source);

	public Category ToDomain(CreateCategoryRequest source)
	{
		return new Category(Guid.Empty, source.Name, source.Description);
	}

	public Category ToDomain(UpdateCategoryRequest source)
	{
		return new Category(source.Id, source.Name, source.Description);
	}
}
