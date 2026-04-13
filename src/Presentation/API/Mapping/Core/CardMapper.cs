using API.Generated.Dtos;
using Domain.Core;
using Riok.Mapperly.Abstractions;

namespace API.Mapping.Core;

[Mapper]
public partial class CardMapper
{
	[MapperIgnoreTarget(nameof(CardResponse.AdditionalProperties))]
	public partial CardResponse ToResponse(Card source);

	public Card ToDomain(CreateCardRequest source)
	{
		return new Card(Guid.Empty, source.CardCode, source.Name, source.IsActive);
	}

	public Card ToDomain(UpdateCardRequest source)
	{
		return new Card(source.Id, source.CardCode, source.Name, source.IsActive);
	}
}
