using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Subcategory.Delete;

public class DeleteSubcategoryCommandHandler(ISubcategoryService subcategoryService) : IRequestHandler<DeleteSubcategoryCommand, bool>
{
	public async Task<bool> Handle(DeleteSubcategoryCommand request, CancellationToken cancellationToken)
	{
		await subcategoryService.DeleteAsync([.. request.Ids], cancellationToken);
		return true;
	}
}
