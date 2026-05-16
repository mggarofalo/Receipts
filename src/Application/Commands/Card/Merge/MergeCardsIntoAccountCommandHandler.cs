using Application.Interfaces.Services;
using Application.Models.Merge;
using Mediator;

namespace Application.Commands.Card.Merge;

public class MergeCardsIntoAccountCommandHandler(IAccountMergeService mergeService)
	: IRequestHandler<MergeCardsIntoAccountCommand, MergeCardsResult>
{
	public async ValueTask<MergeCardsResult> Handle(MergeCardsIntoAccountCommand request, CancellationToken cancellationToken)
	{
		return await mergeService.MergeCardsAsync(
			request.TargetAccountId,
			request.SourceCardIds,
			request.YnabMappingWinnerAccountId,
			cancellationToken);
	}
}
