using Application.Interfaces;

namespace Application.Commands.Reports;

public record RefreshItemSimilarityCommand : ICommand<Mediator.Unit>;
