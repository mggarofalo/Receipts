using API.Mapping.Core;
using Domain.Aggregates;
using Domain.Core;
using Riok.Mapperly.Abstractions;
using Shared.ViewModels.Aggregates;
using Shared.ViewModels.Core;

namespace API.Mapping.Aggregates;

[Mapper]
public partial class TransactionAccountMapper
{
	private readonly TransactionMapper _transactionMapper = new();
	private readonly AccountMapper _accountMapper = new();

	public TransactionAccountVM ToViewModel(TransactionAccount source)
	{
		return new TransactionAccountVM
		{
			Transaction = _transactionMapper.ToViewModel(source.Transaction),
			Account = _accountMapper.ToViewModel(source.Account)
		};
	}

	public TransactionAccount ToDomain(TransactionAccountVM source)
	{
		return new TransactionAccount
		{
			Transaction = _transactionMapper.ToDomain(source.Transaction!),
			Account = _accountMapper.ToDomain(source.Account!)
		};
	}
}
