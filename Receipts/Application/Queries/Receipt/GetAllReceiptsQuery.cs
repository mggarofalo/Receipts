using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Queries.Receipt;

public record GetAllReceiptsQuery() : IQuery<List<Domain.Core.Receipt>>;

public class GetAllReceiptsQueryHandler(IReceiptRepository receiptRepository) : IRequestHandler<GetAllReceiptsQuery, List<Domain.Core.Receipt>>
{
	private readonly IReceiptRepository _receiptRepository = receiptRepository;

	public async Task<List<Domain.Core.Receipt>> Handle(GetAllReceiptsQuery request, CancellationToken cancellationToken)
	{
		return await _receiptRepository.GetAllAsync(cancellationToken);
	}
}
