using Application.Interfaces.Services;
using Domain.Aggregates;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Commands.Transaction.Update;

public class UpdateTransactionCommandHandler(
	ITransactionService transactionService,
	IReceiptService receiptService,
	IReceiptItemService receiptItemService,
	IAdjustmentService adjustmentService) : IRequestHandler<UpdateTransactionCommand, bool>
{
	public async Task<bool> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
	{
		Domain.Core.Transaction existingTransaction = await transactionService.GetByIdAsync(request.Transactions[0].Id, cancellationToken)
			?? throw new InvalidOperationException("Transaction not found");

		Guid receiptId = existingTransaction.ReceiptId;

		Task<Domain.Core.Receipt?> receiptTask = receiptService.GetByIdAsync(receiptId, cancellationToken);
		Task<List<Domain.Core.ReceiptItem>?> itemsTask = receiptItemService.GetByReceiptIdAsync(receiptId, cancellationToken);
		Task<List<Domain.Core.Adjustment>?> adjustmentsTask = adjustmentService.GetByReceiptIdAsync(receiptId, cancellationToken);
		Task<List<Domain.Core.Transaction>?> existingTransactionsTask = transactionService.GetByReceiptIdAsync(receiptId, cancellationToken);

		await Task.WhenAll(receiptTask, itemsTask, adjustmentsTask, existingTransactionsTask);

		Domain.Core.Receipt receipt = receiptTask.Result
			?? throw new InvalidOperationException("Receipt not found");

		ReceiptWithItems receiptWithItems = new()
		{
			Receipt = receipt,
			Items = itemsTask.Result ?? [],
			Adjustments = adjustmentsTask.Result ?? []
		};

		HashSet<Guid> updatedIds = [.. request.Transactions.Select(t => t.Id)];
		decimal unchangedTotal = existingTransactionsTask.Result?
			.Where(t => !updatedIds.Contains(t.Id))
			.Sum(t => t.Amount.Amount) ?? 0;
		decimal updatedTotal = request.Transactions.Sum(t => t.Amount.Amount);
		decimal proposedTotal = unchangedTotal + updatedTotal;

		if (proposedTotal != receiptWithItems.ExpectedTotal.Amount)
		{
			throw new ValidationException(
			[
				new ValidationFailure("Transactions",
					string.Format(Trip.BalanceEquationViolation,
						receiptWithItems.ExpectedTotal.Amount, proposedTotal))
			]);
		}

		await transactionService.UpdateAsync([.. request.Transactions], request.AccountId, receiptId, cancellationToken);
		return true;
	}
}
