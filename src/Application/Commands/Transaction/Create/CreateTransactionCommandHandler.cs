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
		Task<Models.PagedResult<Domain.Core.ReceiptItem>> itemsTask = receiptItemService.GetByReceiptIdAsync(request.ReceiptId, 0, int.MaxValue, Models.SortParams.Default, cancellationToken);
		Task<Models.PagedResult<Domain.Core.Adjustment>> adjustmentsTask = adjustmentService.GetByReceiptIdAsync(request.ReceiptId, 0, int.MaxValue, Models.SortParams.Default, cancellationToken);
		Task<Models.PagedResult<Domain.Core.Transaction>> existingTransactionsTask = transactionService.GetByReceiptIdAsync(request.ReceiptId, 0, int.MaxValue, Models.SortParams.Default, cancellationToken);

		await Task.WhenAll(receiptTask, itemsTask, adjustmentsTask, existingTransactionsTask);

		Domain.Core.Receipt receipt = receiptTask.Result
			?? throw new InvalidOperationException("Receipt not found");

		ReceiptWithItems receiptWithItems = new()
		{
			Receipt = receipt,
			Items = itemsTask.Result.Data,
			Adjustments = adjustmentsTask.Result.Data
		};

		decimal existingTotal = existingTransactionsTask.Result.Data.Sum(t => t.Amount.Amount);
		decimal newTotal = request.Transactions.Sum(t => t.Amount.Amount);
		decimal proposedTotal = existingTotal + newTotal;

		if (Math.Abs(proposedTotal - receiptWithItems.ExpectedTotal.Amount) > 0.01m)
		{
			throw new ValidationException(
			[
				new ValidationFailure("Transactions",
					string.Format(Trip.BalanceEquationViolation,
						receiptWithItems.ExpectedTotal.Amount, proposedTotal))
			]);
		}

		return await transactionService.CreateAsync([.. request.Transactions], request.ReceiptId, cancellationToken);
	}
}
