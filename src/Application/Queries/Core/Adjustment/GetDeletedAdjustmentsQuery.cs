using Application.Interfaces;

namespace Application.Queries.Core.Adjustment;

public record GetDeletedAdjustmentsQuery() : IQuery<List<Domain.Core.Adjustment>>;
