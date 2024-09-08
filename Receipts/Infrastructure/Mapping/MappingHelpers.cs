using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;

namespace Infrastructure.Mapping;

public static class MappingHelpers
{
	public static Guid GetValueFromContext(this ResolutionContext context, string key)
	{
		if (!context.Items.TryGetValue($"{key}", out var idObj) || idObj is not Guid)
		{
			throw new AutoMapperConfigurationException($"{key} must be provided in the mapping context.");
		}

		return (Guid)idObj;
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