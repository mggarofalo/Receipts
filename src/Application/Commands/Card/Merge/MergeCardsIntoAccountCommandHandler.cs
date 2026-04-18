using Application.Interfaces.Services;
using Application.Models.Merge;
using MediatR;

namespace Application.Commands.Card.Merge;

public class MergeCardsIntoAccountCommandHandler(IAccountMergeService mergeService)
	: IRequestHandler<MergeCardsIntoAccountCommand, MergeCardsResult>
{
	public Task<MergeCardsResult> Handle(MergeCardsIntoAccountCommand request, CancellationToken cancellationToken)
	{
		return mergeService.MergeCardsAsync(
			request.TargetAccountId,
			request.SourceCardIds,
			request.YnabMappingWinnerAccountId,
			cancellationToken);
	}
}
