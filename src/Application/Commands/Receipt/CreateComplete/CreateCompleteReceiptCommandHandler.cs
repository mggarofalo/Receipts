using Application.Interfaces.Services;
using Domain.Aggregates;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Commands.Receipt.CreateComplete;

public class CreateCompleteReceiptCommandHandler(
	ICompleteReceiptService completeReceiptService) : IRequestHandler<CreateCompleteReceiptCommand, CreateCompleteReceiptResult>
{
	public async Task<CreateCompleteReceiptResult> Handle(CreateCompleteReceiptCommand request, CancellationToken cancellationToken)
	{
		if (request.Transactions.Count > 0 && request.Items.Count > 0)
		{
			ReceiptWithItems receiptWithItems = new()
			{
				Receipt = request.Receipt,
				Items = [.. request.Items],
				Adjustments = []
			};

			decimal expectedTotal = receiptWithItems.ExpectedTotal.Amount;
			decimal transactionTotal = request.Transactions.Sum(t => t.Amount.Amount);

			if (expectedTotal != transactionTotal)
			{
				throw new ValidationException(
				[
					new ValidationFailure("Transactions",
						string.Format(Trip.BalanceEquationViolation,
							expectedTotal, transactionTotal))
				]);
			}
		}

		return await completeReceiptService.CreateAsync(
			request.Receipt,
			[.. request.Transactions],
			[.. request.Items],
			cancellationToken);
	}
}
