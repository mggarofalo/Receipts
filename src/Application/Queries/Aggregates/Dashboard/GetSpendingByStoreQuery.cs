using Application.Interfaces;
using Application.Models.Dashboard;

namespace Application.Queries.Aggregates.Dashboard;

public record GetSpendingByStoreQuery(DateOnly StartDate, DateOnly EndDate) : IQuery<SpendingByStoreResult>;
