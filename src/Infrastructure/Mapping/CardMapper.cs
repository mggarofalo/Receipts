using Domain.Core;
using Infrastructure.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Infrastructure.Mapping;

[Mapper]
public partial class CardMapper
{
	public partial CardEntity ToEntity(Card source);

	public partial Card ToDomain(CardEntity source);
}
