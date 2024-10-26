using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.Receipt;

public class GetReceiptByIdQueryHandler(IReceiptService receiptRepository) : IRequestHandler<GetReceiptByIdQuery, Domain.Core.Receipt?>
{
	public async Task<Domain.Core.Receipt?> Handle(GetReceiptByIdQuery request, CancellationToken cancellationToken)
	{
		return await receiptRepository.GetByIdAsync(request.Id, cancellationToken);
	}
}
