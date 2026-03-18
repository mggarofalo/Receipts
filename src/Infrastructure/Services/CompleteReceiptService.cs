using Application.Commands.Receipt.CreateComplete;
using Application.Interfaces.Services;
using Domain.Core;
using Infrastructure.Entities.Core;
using Infrastructure.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class CompleteReceiptService(
	IDbContextFactory<ApplicationDbContext> dbContextFactory,
	ReceiptMapper receiptMapper,
	TransactionMapper transactionMapper,
	ReceiptItemMapper receiptItemMapper) : ICompleteReceiptService
{
	public async Task<CreateCompleteReceiptResult> CreateCompleteReceiptAsync(
		Receipt receipt,
		List<Transaction> transactions,
		List<ReceiptItem> items,
		CancellationToken cancellationToken)
	{
		await using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

		// Map and persist receipt
		ReceiptEntity receiptEntity = receiptMapper.ToEntity(receipt);
		if (receiptEntity.Id == Guid.Empty)
		{
			receiptEntity.Id = Guid.NewGuid();
		}

		dbContext.Receipts.Add(receiptEntity);

		// Map and persist transactions with FK
		List<TransactionEntity> transactionEntities = [.. transactions.Select(t =>
		{
			TransactionEntity entity = transactionMapper.ToEntity(t);
			entity.ReceiptId = receiptEntity.Id;
			entity.AccountId = t.AccountId;
			if (entity.Id == Guid.Empty)
			{
				entity.Id = Guid.NewGuid();
			}

			return entity;
		})];

		dbContext.Transactions.AddRange(transactionEntities);

		// Map and persist items with FK
		List<ReceiptItemEntity> itemEntities = [.. items.Select(i =>
		{
			ReceiptItemEntity entity = receiptItemMapper.ToEntity(i);
			entity.ReceiptId = receiptEntity.Id;
			if (entity.Id == Guid.Empty)
			{
				entity.Id = Guid.NewGuid();
			}

			return entity;
		})];

		dbContext.ReceiptItems.AddRange(itemEntities);

		// Single SaveChangesAsync — all or nothing
		await dbContext.SaveChangesAsync(cancellationToken);

		// Map back to domain
		Receipt createdReceipt = receiptMapper.ToDomain(receiptEntity);
		List<Transaction> createdTransactions = [.. transactionEntities.Select(transactionMapper.ToDomain)];
		List<ReceiptItem> createdItems = [.. itemEntities.Select(receiptItemMapper.ToDomain)];

		return new CreateCompleteReceiptResult(createdReceipt, createdTransactions, createdItems);
	}
}
