using Domain.Core;
using Infrastructure.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Infrastructure.Mapping;

[Mapper]
public partial class CardMapper
{
	[MapperIgnoreTarget(nameof(CardEntity.ParentAccount))]
	public partial CardEntity ToEntity(Card source);

	[MapperIgnoreSource(nameof(CardEntity.ParentAccount))]
	public partial Card ToDomain(CardEntity source);
}
