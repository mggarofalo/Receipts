using Application.Interfaces;
using Application.Models.Dashboard;

namespace Application.Queries.Aggregates.Dashboard;

public record GetSpendingOverTimeQuery(DateOnly StartDate, DateOnly EndDate, string Granularity) : IQuery<SpendingOverTimeResult>;
