using Application.Interfaces;
using Application.Models.Reports;

namespace Application.Queries.Aggregates.Reports;

public record GetItemDescriptionsQuery(
	string Search,
	bool CategoryOnly,
	int Limit) : IQuery<ItemDescriptionResult>;
