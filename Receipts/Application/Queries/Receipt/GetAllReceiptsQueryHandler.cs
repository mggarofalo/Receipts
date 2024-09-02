using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Queries.Receipt;

public class GetAllReceiptsQueryHandler(IReceiptRepository receiptRepository) : IRequestHandler<GetAllReceiptsQuery, List<Domain.Core.Receipt>>
{
	public async Task<List<Domain.Core.Receipt>> Handle(GetAllReceiptsQuery request, CancellationToken cancellationToken)
	{
		return await receiptRepository.GetAllAsync(cancellationToken);
	}
}
