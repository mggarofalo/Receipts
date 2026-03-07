using Application.Interfaces;
using Application.Models;

namespace Application.Queries.Core.Adjustment;

public record GetDeletedAdjustmentsQuery(int Offset, int Limit, SortParams Sort) : IQuery<PagedResult<Domain.Core.Adjustment>>;
