using Application.Interfaces;

namespace Application.Queries.Core.Adjustment;

public record GetAllAdjustmentsQuery() : IQuery<List<Domain.Core.Adjustment>>;
