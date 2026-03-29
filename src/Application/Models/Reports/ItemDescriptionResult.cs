namespace Application.Models.Reports;

public record ItemDescriptionItem(
	string Description,
	string Category,
	int Occurrences);

public record ItemDescriptionResult(
	List<ItemDescriptionItem> Items);
