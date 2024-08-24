using Application.Common;
using Application.Interfaces;
using AutoMapper;
using MediatR;

namespace Application.Commands.Receipt;

public record UpdateReceiptCommand(Guid Id, string Location, DateTime Date, decimal TaxAmount, decimal TotalAmount, string? Description) : ICommand<bool>;

public class UpdateReceiptCommandHandler(IReceiptRepository receiptRepository, IMapper mapper) : IRequestHandler<UpdateReceiptCommand, bool>
{
	private readonly IReceiptRepository _receiptRepository = receiptRepository;
	private readonly IMapper _mapper = mapper;

	public async Task<bool> Handle(UpdateReceiptCommand request, CancellationToken cancellationToken)
	{
		Domain.Core.Receipt? existingReceipt = await _receiptRepository.GetByIdAsync(request.Id, cancellationToken);

		if (existingReceipt == null)
		{
			return false;
		}

		Domain.Core.Receipt updatedReceipt = _mapper.Map<Domain.Core.Receipt>(request);

		_ = await _receiptRepository.UpdateAsync(updatedReceipt, cancellationToken);
		await _receiptRepository.SaveChangesAsync(cancellationToken);

		return true;
	}
}
