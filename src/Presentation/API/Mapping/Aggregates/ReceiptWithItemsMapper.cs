using API.Mapping.Core;
using Domain.Aggregates;
using Domain.Core;
using Riok.Mapperly.Abstractions;
using Shared.ViewModels.Aggregates;
using Shared.ViewModels.Core;

namespace API.Mapping.Aggregates;

[Mapper]
public partial class ReceiptWithItemsMapper
{
	private readonly ReceiptMapper _receiptMapper = new();
	private readonly ReceiptItemMapper _receiptItemMapper = new();

	public ReceiptWithItemsVM ToViewModel(ReceiptWithItems source)
	{
		return new ReceiptWithItemsVM
		{
			Receipt = _receiptMapper.ToViewModel(source.Receipt),
			Items = source.Items.Select(_receiptItemMapper.ToViewModel).ToList()
		};
	}

	public ReceiptWithItems ToDomain(ReceiptWithItemsVM source)
	{
		return new ReceiptWithItems
		{
			Receipt = _receiptMapper.ToDomain(source.Receipt!),
			Items = source.Items!.Select(_receiptItemMapper.ToDomain).ToList()
		};
	}
}
