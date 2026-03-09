using Application.Interfaces;
using Application.Models.Dashboard;

namespace Application.Queries.Aggregates.Dashboard;

public record GetDashboardSummaryQuery(DateOnly StartDate, DateOnly EndDate) : IQuery<DashboardSummaryResult>;
