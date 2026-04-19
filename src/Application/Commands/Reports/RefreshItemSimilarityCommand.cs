using Application.Interfaces;
using MediatR;

namespace Application.Commands.Reports;

public record RefreshItemSimilarityCommand : ICommand<Unit>;
