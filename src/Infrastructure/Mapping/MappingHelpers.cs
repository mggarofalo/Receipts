using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;

namespace Infrastructure.Mapping;

public static class MappingHelpers
{
	public static Guid GetValueFromContext(this ResolutionContext context, string key)
	{
		// This method can assume that the key will always be present in the mapping context.
		// If it's not present, the mapping will fail prior to this call being made. If it
		// does not fail prior to this call, then the new mapping was incorrect and we'll
		// throw an InvalidOperationException anyway.

		Guid? id = context.Items.GetValueOrDefault(key) as Guid?;
		return id!.Value;
	}

	public static ReceiptItemEntity MapToReceiptItemEntity(this IMapper mapper, ReceiptItem source, Guid receiptId)
	{
		return mapper.Map<ReceiptItemEntity>(source, opt =>
		{
			opt.Items.Add($"{nameof(ReceiptItemEntity.ReceiptId)}", receiptId);
		});
	}

	public static TransactionEntity MapToTransactionEntity(this IMapper mapper, Transaction source, Guid receiptId, Guid accountId)
	{
		return mapper.Map<TransactionEntity>(source, opt =>
		{
			opt.Items.Add($"{nameof(TransactionEntity.ReceiptId)}", receiptId);
			opt.Items.Add($"{nameof(TransactionEntity.AccountId)}", accountId);
		});
	}
}