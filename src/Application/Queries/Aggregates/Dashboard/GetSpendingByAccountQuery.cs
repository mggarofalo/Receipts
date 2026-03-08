using Application.Interfaces;
using Application.Models.Dashboard;

namespace Application.Queries.Aggregates.Dashboard;

public record GetSpendingByAccountQuery(DateOnly StartDate, DateOnly EndDate) : IQuery<SpendingByAccountResult>;
