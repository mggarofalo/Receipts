using API.Generated.Dtos;
using API.Mapping.Core;
using Domain.Aggregates;
using Riok.Mapperly.Abstractions;

namespace API.Mapping.Aggregates;

[Mapper]
public partial class ReceiptWithItemsMapper
{
	private readonly ReceiptMapper _receiptMapper = new();
	private readonly ReceiptItemMapper _receiptItemMapper = new();

	public ReceiptWithItemsResponse ToResponse(ReceiptWithItems source)
	{
		return new ReceiptWithItemsResponse
		{
			Receipt = _receiptMapper.ToResponse(source.Receipt),
			Items = source.Items.Select(_receiptItemMapper.ToResponse).ToList()
		};
	}
}
