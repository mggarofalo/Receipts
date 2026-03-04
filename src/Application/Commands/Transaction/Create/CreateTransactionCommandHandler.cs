using Application.Interfaces.Services;
using Domain.Aggregates;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Commands.Transaction.Create;

public class CreateTransactionCommandHandler(
	ITransactionService transactionService,
	IReceiptService receiptService,
	IReceiptItemService receiptItemService,
	IAdjustmentService adjustmentService) : IRequestHandler<CreateTransactionCommand, List<Domain.Core.Transaction>>
{
	public async Task<List<Domain.Core.Transaction>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
	{
		Task<Domain.Core.Receipt?> receiptTask = receiptService.GetByIdAsync(request.ReceiptId, cancellationToken);
		Task<List<Domain.Core.ReceiptItem>?> itemsTask = receiptItemService.GetByReceiptIdAsync(request.ReceiptId, cancellationToken);
		Task<List<Domain.Core.Adjustment>?> adjustmentsTask = adjustmentService.GetByReceiptIdAsync(request.ReceiptId, cancellationToken);
		Task<List<Domain.Core.Transaction>?> existingTransactionsTask = transactionService.GetByReceiptIdAsync(request.ReceiptId, cancellationToken);

		await Task.WhenAll(receiptTask, itemsTask, adjustmentsTask, existingTransactionsTask);

		Domain.Core.Receipt receipt = receiptTask.Result
			?? throw new InvalidOperationException("Receipt not found");

		ReceiptWithItems receiptWithItems = new()
		{
			Receipt = receipt,
			Items = itemsTask.Result ?? [],
			Adjustments = adjustmentsTask.Result ?? []
		};

		decimal existingTotal = existingTransactionsTask.Result?.Sum(t => t.Amount.Amount) ?? 0;
		decimal newTotal = request.Transactions.Sum(t => t.Amount.Amount);
		decimal proposedTotal = existingTotal + newTotal;

		if (proposedTotal != receiptWithItems.ExpectedTotal.Amount)
		{
			throw new ValidationException(
			[
				new ValidationFailure("Transactions",
					string.Format(Trip.BalanceEquationViolation,
						receiptWithItems.ExpectedTotal.Amount, proposedTotal))
			]);
		}

		return await transactionService.CreateAsync([.. request.Transactions], request.ReceiptId, request.AccountId, cancellationToken);
	}
}
