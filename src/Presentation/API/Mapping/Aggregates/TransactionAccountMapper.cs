using API.Generated.Dtos;
using API.Mapping.Core;
using Domain.Aggregates;
using Riok.Mapperly.Abstractions;

namespace API.Mapping.Aggregates;

[Mapper]
public partial class TransactionAccountMapper
{
	private readonly TransactionMapper _transactionMapper = new();
	private readonly CardMapper _cardMapper = new();

	public TransactionAccountResponse ToResponse(TransactionAccount source)
	{
		return new TransactionAccountResponse
		{
			Transaction = _transactionMapper.ToResponse(source.Transaction),
			Account = _cardMapper.ToResponse(source.Account)
		};
	}
}
