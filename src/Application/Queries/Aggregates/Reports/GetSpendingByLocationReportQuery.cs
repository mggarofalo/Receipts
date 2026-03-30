using Application.Interfaces;
using Application.Models.Reports;

namespace Application.Queries.Aggregates.Reports;

public record GetSpendingByLocationReportQuery(
	DateOnly? StartDate,
	DateOnly? EndDate,
	string SortBy,
	string SortDirection,
	int Page,
	int PageSize) : IQuery<SpendingByLocationResult>;
