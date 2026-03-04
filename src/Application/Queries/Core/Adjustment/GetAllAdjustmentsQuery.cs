using Application.Interfaces;
using Application.Models;

namespace Application.Queries.Core.Adjustment;

public record GetAllAdjustmentsQuery(int Offset, int Limit) : IQuery<PagedResult<Domain.Core.Adjustment>>;
