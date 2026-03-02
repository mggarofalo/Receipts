using API.Generated.Dtos;
using Common;
using Domain;
using Domain.Core;
using Riok.Mapperly.Abstractions;

namespace API.Mapping.Core;

[Mapper]
public partial class AdjustmentMapper
{
	[MapProperty(nameof(Adjustment.Amount.Amount), nameof(AdjustmentResponse.Amount))]
	[MapperIgnoreTarget(nameof(AdjustmentResponse.AdditionalProperties))]
	public partial AdjustmentResponse ToResponse(Adjustment source);

	public Adjustment ToDomain(CreateAdjustmentRequest source)
	{
		return new Adjustment(
			Guid.Empty,
			Enum.Parse<AdjustmentType>(source.Type, ignoreCase: true),
			new Money((decimal)source.Amount, Currency.USD),
			source.Description
		);
	}

	public Adjustment ToDomain(UpdateAdjustmentRequest source)
	{
		return new Adjustment(
			source.Id,
			Enum.Parse<AdjustmentType>(source.Type, ignoreCase: true),
			new Money((decimal)source.Amount, Currency.USD),
			source.Description
		);
	}

	private double MapDecimalToDouble(decimal value) => (double)value;
	private string MapAdjustmentTypeToString(AdjustmentType value) => value.ToString();
}
