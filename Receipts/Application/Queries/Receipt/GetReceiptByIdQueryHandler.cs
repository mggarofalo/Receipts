using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Queries.Receipt;

public class GetReceiptByIdQueryHandler(IReceiptRepository receiptRepository) : IRequestHandler<GetReceiptByIdQuery, Domain.Core.Receipt?>
{
	public async Task<Domain.Core.Receipt?> Handle(GetReceiptByIdQuery request, CancellationToken cancellationToken)
	{
		return await receiptRepository.GetByIdAsync(request.Id, cancellationToken);
	}
}
