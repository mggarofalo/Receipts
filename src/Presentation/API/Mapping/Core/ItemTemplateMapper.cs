using API.Generated.Dtos;
using Common;
using Domain;
using Domain.Core;
using Riok.Mapperly.Abstractions;

namespace API.Mapping.Core;

[Mapper]
public partial class ItemTemplateMapper
{
	[MapperIgnoreTarget(nameof(ItemTemplateResponse.AdditionalProperties))]
	[MapperIgnoreSource(nameof(ItemTemplate.DefaultUnitPrice))]
	[MapperIgnoreTarget(nameof(ItemTemplateResponse.DefaultUnitPrice))]
	[MapperIgnoreTarget(nameof(ItemTemplateResponse.DefaultUnitPriceCurrency))]
	public partial ItemTemplateResponse ToResponsePartial(ItemTemplate source);

	public ItemTemplateResponse ToResponse(ItemTemplate source)
	{
		ItemTemplateResponse response = ToResponsePartial(source);
		if (source.DefaultUnitPrice != null)
		{
			response.DefaultUnitPrice = (double)source.DefaultUnitPrice.Amount;
			response.DefaultUnitPriceCurrency = source.DefaultUnitPrice.Currency.ToString();
		}
		return response;
	}

	public ItemTemplate ToDomain(CreateItemTemplateRequest source)
	{
		Money? defaultUnitPrice = source.DefaultUnitPrice.HasValue
			? new Money((decimal)source.DefaultUnitPrice.Value, Currency.USD)
			: null;

		return new ItemTemplate(
			Guid.Empty,
			source.Name,
			source.DefaultCategory,
			source.DefaultSubcategory,
			defaultUnitPrice,
			source.DefaultPricingMode,
			source.DefaultItemCode,
			source.Description
		);
	}

	public ItemTemplate ToDomain(UpdateItemTemplateRequest source)
	{
		Money? defaultUnitPrice = source.DefaultUnitPrice.HasValue
			? new Money((decimal)source.DefaultUnitPrice.Value, Currency.USD)
			: null;

		return new ItemTemplate(
			source.Id,
			source.Name,
			source.DefaultCategory,
			source.DefaultSubcategory,
			defaultUnitPrice,
			source.DefaultPricingMode,
			source.DefaultItemCode,
			source.Description
		);
	}
}
