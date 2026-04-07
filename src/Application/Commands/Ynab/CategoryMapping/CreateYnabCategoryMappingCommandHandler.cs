using Application.Exceptions;
using Application.Interfaces.Services;
using Application.Models.Ynab;
using MediatR;

namespace Application.Commands.Ynab.CategoryMapping;

public class CreateYnabCategoryMappingCommandHandler(IYnabCategoryMappingService service) : IRequestHandler<CreateYnabCategoryMappingCommand, YnabCategoryMappingDto>
{
	public async Task<YnabCategoryMappingDto> Handle(CreateYnabCategoryMappingCommand request, CancellationToken cancellationToken)
	{
		// Cross-entity validation: check for duplicate ReceiptsCategory (case-sensitive)
		YnabCategoryMappingDto? existing = await service.GetByReceiptsCategoryAsync(request.ReceiptsCategory, cancellationToken);
		if (existing is not null)
		{
			throw new DuplicateEntityException($"A mapping for receipts category '{request.ReceiptsCategory}' already exists.");
		}

		// CreateAsync catches DbUpdateException (unique constraint) and converts to
		// DuplicateEntityException, guarding against the TOCTOU race where two concurrent
		// requests both pass the existence check above.
		return await service.CreateAsync(
			request.ReceiptsCategory,
			request.YnabCategoryId,
			request.YnabCategoryName,
			request.YnabCategoryGroupName,
			request.YnabBudgetId,
			cancellationToken);
	}
}
