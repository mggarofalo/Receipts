using Application.Interfaces;
using Application.Models;

namespace Application.Queries.Core.Receipt;

public record GetAllReceiptsQuery(int Offset, int Limit) : IQuery<PagedResult<Domain.Core.Receipt>>;
