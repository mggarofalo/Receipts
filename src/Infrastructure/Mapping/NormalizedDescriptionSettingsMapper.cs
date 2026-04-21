using Domain.NormalizedDescriptions;
using Infrastructure.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Infrastructure.Mapping;

[Mapper]
public partial class NormalizedDescriptionSettingsMapper
{
	public partial NormalizedDescriptionSettingsEntity ToEntity(NormalizedDescriptionSettings source);

	public partial NormalizedDescriptionSettings ToDomain(NormalizedDescriptionSettingsEntity source);
}
