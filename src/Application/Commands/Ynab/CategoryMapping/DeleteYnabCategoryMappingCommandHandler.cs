using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Ynab.CategoryMapping;

public class DeleteYnabCategoryMappingCommandHandler(IYnabCategoryMappingService service) : IRequestHandler<DeleteYnabCategoryMappingCommand, Unit>
{
	public async ValueTask<Unit> Handle(DeleteYnabCategoryMappingCommand request, CancellationToken cancellationToken)
	{
		await service.DeleteAsync(request.Id, cancellationToken);
		return Unit.Value;
	}
}
