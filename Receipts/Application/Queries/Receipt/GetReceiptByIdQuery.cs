using Application.Interfaces;
using MediatR;

namespace Application.Queries.Receipt;

public record GetReceiptByIdQuery(Guid Id) : IQuery<Domain.Core.Receipt?>;

public class GetReceiptByIdQueryHandler(IReceiptRepository receiptRepository) : IRequestHandler<GetReceiptByIdQuery, Domain.Core.Receipt?>
{
	private readonly IReceiptRepository _receiptRepository = receiptRepository;

	public async Task<Domain.Core.Receipt?> Handle(GetReceiptByIdQuery request, CancellationToken cancellationToken)
	{
		return await _receiptRepository.GetByIdAsync(request.Id, cancellationToken);
	}
}
