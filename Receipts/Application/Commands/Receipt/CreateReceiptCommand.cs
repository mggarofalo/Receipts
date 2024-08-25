using Application.Common;
using Application.Interfaces;
using AutoMapper;
using MediatR;

namespace Application.Commands.Receipt;

public record CreateReceiptCommand(string Location, DateOnly Date, decimal TaxAmount, string? Description) : ICommand<Guid>;

public class CreateReceiptCommandHandler(IReceiptRepository receiptRepository, IMapper mapper) : IRequestHandler<CreateReceiptCommand, Guid>
{
	private readonly IReceiptRepository _receiptRepository = receiptRepository;
	private readonly IMapper _mapper = mapper;

	public async Task<Guid> Handle(CreateReceiptCommand request, CancellationToken cancellationToken)
	{
		Domain.Core.Receipt receipt = _mapper.Map<Domain.Core.Receipt>(request);
		Domain.Core.Receipt createdEntity = await _receiptRepository.CreateAsync(receipt, cancellationToken);
		await _receiptRepository.SaveChangesAsync(cancellationToken);
		return createdEntity.Id!.Value;
	}
}
