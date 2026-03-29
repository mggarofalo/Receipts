using Application.Interfaces;
using Application.Models.Reports;

namespace Application.Queries.Aggregates.Reports;

public record GetOutOfBalanceReportQuery(
	string SortBy,
	string SortDirection,
	int Page,
	int PageSize) : IQuery<OutOfBalanceResult>;
