using Application.Interfaces;
using Application.Models.Dashboard;

namespace Application.Queries.Aggregates.Dashboard;

public record GetSpendingByCategoryQuery(DateOnly StartDate, DateOnly EndDate, int Limit) : IQuery<SpendingByCategoryResult>;
