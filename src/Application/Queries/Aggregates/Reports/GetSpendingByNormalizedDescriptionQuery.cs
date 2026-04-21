using Application.Interfaces;
using Application.Models.Reports;

namespace Application.Queries.Aggregates.Reports;

public record GetSpendingByNormalizedDescriptionQuery(
	DateTimeOffset? From,
	DateTimeOffset? To) : IQuery<SpendingByNormalizedDescriptionResult>;
