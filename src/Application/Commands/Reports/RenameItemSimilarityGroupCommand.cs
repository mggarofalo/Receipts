using Application.Interfaces;

namespace Application.Commands.Reports;

public record RenameItemSimilarityGroupCommand(
	List<Guid> ItemIds,
	string NewDescription) : ICommand<int>;
