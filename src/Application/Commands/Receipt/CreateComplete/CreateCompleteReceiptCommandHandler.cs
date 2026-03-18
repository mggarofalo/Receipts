using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Receipt.CreateComplete;

public class CreateCompleteReceiptCommandHandler(ICompleteReceiptService completeReceiptService)
	: IRequestHandler<CreateCompleteReceiptCommand, CreateCompleteReceiptResult>
{
	public async Task<CreateCompleteReceiptResult> Handle(CreateCompleteReceiptCommand request, CancellationToken cancellationToken)
	{
		return await completeReceiptService.CreateCompleteReceiptAsync(
			request.Receipt,
			[.. request.Transactions],
			[.. request.Items],
			cancellationToken);
	}
}
