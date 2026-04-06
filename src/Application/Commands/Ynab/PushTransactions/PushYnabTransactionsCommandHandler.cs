using Application.Interfaces.Services;
using Application.Models;
using Application.Models.Ynab;
using Application.Utilities;
using Common;
using Domain.Aggregates;
using MediatR;

namespace Application.Commands.Ynab.PushTransactions;

public class PushYnabTransactionsCommandHandler(
	IReceiptService receiptService,
	IReceiptItemService receiptItemService,
	IAdjustmentService adjustmentService,
	ITransactionService transactionService,
	IYnabCategoryMappingService categoryMappingService,
	IYnabAccountMappingService accountMappingService,
	IYnabBudgetSelectionService budgetSelectionService,
	IYnabSyncRecordService syncRecordService,
	IYnabApiClient ynabApiClient,
	IYnabSplitCalculator splitCalculator) : IRequestHandler<PushYnabTransactionsCommand, PushYnabTransactionsResult>
{
	public async Task<PushYnabTransactionsResult> Handle(PushYnabTransactionsCommand request, CancellationToken cancellationToken)
	{
		// 1. Load the receipt and related data
		Domain.Core.Receipt? receipt = await receiptService.GetByIdAsync(request.ReceiptId, cancellationToken);
		if (receipt is null)
		{
			return new PushYnabTransactionsResult(false, [], Error: "Receipt not found.");
		}

		// Currency guard: USD only (V1)
		if (receipt.TaxAmount.Currency != Currency.USD)
		{
			return new PushYnabTransactionsResult(false, [], Error: "Only USD receipts are supported for YNAB sync.");
		}

		PagedResult<Domain.Core.ReceiptItem> itemsResult = await receiptItemService.GetByReceiptIdAsync(
			request.ReceiptId, 0, 10000, new SortParams("Description", "asc"), cancellationToken);
		List<Domain.Core.ReceiptItem> items = itemsResult.Data.ToList();

		if (items.Count == 0)
		{
			return new PushYnabTransactionsResult(false, [], Error: "Receipt has no items.");
		}

		// Currency guard on items
		if (items.Any(i => i.TotalAmount.Currency != Currency.USD))
		{
			return new PushYnabTransactionsResult(false, [], Error: "Only USD receipts are supported for YNAB sync.");
		}

		PagedResult<Domain.Core.Adjustment> adjResult = await adjustmentService.GetByReceiptIdAsync(
			request.ReceiptId, 0, 10000, new SortParams("Type", "asc"), cancellationToken);
		List<Domain.Core.Adjustment> adjustments = adjResult.Data.ToList();

		List<TransactionAccount> transactionAccounts = await transactionService.GetTransactionAccountsByReceiptIdAsync(
			request.ReceiptId, cancellationToken);
		List<Domain.Core.Transaction> transactions = transactionAccounts.Select(ta => ta.Transaction).ToList();

		if (transactions.Count == 0)
		{
			return new PushYnabTransactionsResult(false, [], Error: "Receipt has no transactions.");
		}

		// 2. Check all categories are mapped (fail-fast)
		List<string> distinctCategories = items.Select(i => i.Category).Distinct().ToList();
		List<YnabCategoryMappingDto> allMappings = await categoryMappingService.GetAllAsync(cancellationToken);
		Dictionary<string, string> categoryToYnabId = allMappings
			.ToDictionary(m => m.ReceiptsCategory, m => m.YnabCategoryId);

		List<string> unmapped = distinctCategories.Where(c => !categoryToYnabId.ContainsKey(c)).ToList();
		if (unmapped.Count > 0)
		{
			return new PushYnabTransactionsResult(false, [], UnmappedCategories: unmapped, Error: "Unmapped categories found.");
		}

		// 3. Get selected budget
		string? budgetId = await budgetSelectionService.GetSelectedBudgetIdAsync(cancellationToken);
		if (string.IsNullOrEmpty(budgetId))
		{
			return new PushYnabTransactionsResult(false, [], Error: "No YNAB budget selected.");
		}

		// 4. Get account mappings for the transactions
		List<YnabAccountMappingDto> accountMappingsList = await accountMappingService.GetAllAsync(cancellationToken);
		Dictionary<Guid, string> accountToYnabId = accountMappingsList
			.ToDictionary(m => m.ReceiptsAccountId, m => m.YnabAccountId);

		// Check all transaction accounts have YNAB mappings
		List<Guid> unmappedAccountIds = transactions
			.Select(t => t.AccountId)
			.Distinct()
			.Where(id => !accountToYnabId.ContainsKey(id))
			.ToList();

		if (unmappedAccountIds.Count > 0)
		{
			return new PushYnabTransactionsResult(false, [], Error: "Some transaction accounts are not mapped to YNAB accounts.");
		}

		// 5. Skip already-synced transactions (allows retry after partial push)
		HashSet<Guid> alreadySyncedIds = [];
		foreach (Domain.Core.Transaction tx in transactions)
		{
			YnabSyncRecordDto? existingSync = await syncRecordService.GetByTransactionAndTypeAsync(
				tx.Id, YnabSyncType.TransactionPush, cancellationToken);
			if (existingSync is not null && existingSync.SyncStatus == YnabSyncStatus.Synced)
			{
				alreadySyncedIds.Add(tx.Id);
			}
		}

		// 6. Build ReceiptWithItems aggregate
		ReceiptWithItems receiptWithItems = new()
		{
			Receipt = receipt,
			Items = items,
			Adjustments = adjustments,
		};

		// 7. Compute waterfall splits
		YnabSplitResult splitResult = splitCalculator.ComputeWaterfallSplits(
			receiptWithItems, transactions, categoryToYnabId);

		// 8. Create YNAB transactions and track sync
		List<PushedTransactionInfo> pushedTransactions = [];
		Dictionary<(long Milliunits, DateOnly Date), int> importIdOccurrences = [];

		foreach (YnabTransactionSplit txSplit in splitResult.TransactionSplits)
		{
			Domain.Core.Transaction localTx = transactions.First(t => t.Id == txSplit.LocalTransactionId);

			// Skip already-synced transactions (Bug 1: allows retry after partial push)
			if (alreadySyncedIds.Contains(localTx.Id))
			{
				continue;
			}

			string ynabAccountId = accountToYnabId[localTx.AccountId];

			// Compute import_id for deduplication (includes receipt prefix to avoid cross-receipt collision)
			(long Milliunits, DateOnly Date) importIdKey = (txSplit.TotalMilliunits, localTx.Date);
			int occurrence = importIdOccurrences.TryGetValue(importIdKey, out int current) ? current + 1 : 1;
			importIdOccurrences[importIdKey] = occurrence;
			string importId = YnabImportId.Generate(txSplit.TotalMilliunits, localTx.Date, request.ReceiptId, occurrence);

			try
			{
				// Create sync record (Pending) — inside try so DB failure is caught (Bug 3)
				YnabSyncRecordDto syncRecord = await syncRecordService.CreateAsync(
					localTx.Id, budgetId, YnabSyncType.TransactionPush, cancellationToken);

				// Build sub-transactions
				List<YnabSubTransaction>? subTransactions = null;
				string? categoryId = null;

				if (txSplit.SubTransactions.Count == 1)
				{
					// Single category — no split needed
					categoryId = txSplit.SubTransactions[0].YnabCategoryId;
				}
				else if (txSplit.SubTransactions.Count > 1)
				{
					subTransactions = txSplit.SubTransactions
						.Select(st => new YnabSubTransaction(st.Milliunits, st.YnabCategoryId, null))
						.ToList();
				}

				YnabCreateTransactionRequest ynabRequest = new(
					AccountId: ynabAccountId,
					Date: localTx.Date,
					Amount: txSplit.TotalMilliunits,
					Memo: $"Receipt: {receipt.Location} ({receipt.Date:yyyy-MM-dd})",
					PayeeName: receipt.Location,
					CategoryId: categoryId,
					Approved: false,
					SubTransactions: subTransactions,
					ImportId: importId);

				YnabCreateTransactionResponse ynabResponse = await ynabApiClient.CreateTransactionAsync(
					budgetId, ynabRequest, cancellationToken);

				// Update sync record to Synced — separate error handling (Bug 6)
				try
				{
					await syncRecordService.UpdateStatusAsync(
						syncRecord.Id, YnabSyncStatus.Synced, ynabResponse.TransactionId, null, cancellationToken);
				}
				catch (Exception statusEx)
				{
					// YNAB TX already created; don't mark as Failed. Return success with warning.
					pushedTransactions.Add(new PushedTransactionInfo(
						localTx.Id,
						ynabResponse.TransactionId,
						txSplit.TotalMilliunits,
						txSplit.SubTransactions.Count));

					return new PushYnabTransactionsResult(true, pushedTransactions,
						Error: $"YNAB transaction created but sync record update failed for transaction {localTx.Id}: {statusEx.Message}");
				}

				pushedTransactions.Add(new PushedTransactionInfo(
					localTx.Id,
					ynabResponse.TransactionId,
					txSplit.TotalMilliunits,
					txSplit.SubTransactions.Count));
			}
			catch (Exception ex)
			{
				return new PushYnabTransactionsResult(false, pushedTransactions,
					Error: $"Failed to push YNAB transaction for local transaction {localTx.Id}: {ex.Message}");
			}
		}

		return new PushYnabTransactionsResult(true, pushedTransactions);
	}
}
