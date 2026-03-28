using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Subcategory.Delete;

public class DeleteSubcategoryCommandHandler(ISubcategoryService subcategoryService) : IRequestHandler<DeleteSubcategoryCommand, bool>
{
	public async Task<bool> Handle(DeleteSubcategoryCommand request, CancellationToken cancellationToken)
	{
		bool exists = await subcategoryService.ExistsAsync(request.Id, cancellationToken);
		if (!exists)
		{
			return false;
		}

		await subcategoryService.DeleteAsync(request.Id, cancellationToken);
		return true;
	}
}
