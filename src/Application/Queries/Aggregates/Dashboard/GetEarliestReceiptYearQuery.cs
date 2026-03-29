using Application.Interfaces;

namespace Application.Queries.Aggregates.Dashboard;

public record GetEarliestReceiptYearQuery : IQuery<int>;
