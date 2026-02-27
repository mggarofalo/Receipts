using API.Generated.Dtos;
using Domain.Core;
using Riok.Mapperly.Abstractions;

namespace API.Mapping.Core;

[Mapper]
public partial class SubcategoryMapper
{
	[MapperIgnoreTarget(nameof(SubcategoryResponse.AdditionalProperties))]
	public partial SubcategoryResponse ToResponse(Subcategory source);

	public Subcategory ToDomain(CreateSubcategoryRequest source)
	{
		return new Subcategory(Guid.Empty, source.Name, source.CategoryId, source.Description);
	}

	public Subcategory ToDomain(UpdateSubcategoryRequest source)
	{
		return new Subcategory(source.Id, source.Name, source.CategoryId, source.Description);
	}
}
