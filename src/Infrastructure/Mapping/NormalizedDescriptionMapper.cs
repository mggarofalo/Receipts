using Domain.NormalizedDescriptions;
using Infrastructure.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Infrastructure.Mapping;

[Mapper]
public partial class NormalizedDescriptionMapper
{
	[MapperIgnoreTarget(nameof(NormalizedDescriptionEntity.Embedding))]
	[MapperIgnoreTarget(nameof(NormalizedDescriptionEntity.EmbeddingModelVersion))]
	public partial NormalizedDescriptionEntity ToEntity(NormalizedDescription source);

	[MapperIgnoreSource(nameof(NormalizedDescriptionEntity.Embedding))]
	[MapperIgnoreSource(nameof(NormalizedDescriptionEntity.EmbeddingModelVersion))]
	public partial NormalizedDescription ToDomain(NormalizedDescriptionEntity source);
}
