namespace Application.Tests.Commands;

public interface ICommandTests<T>
{
	List<T> GenerateItemsForTest();
	void Command_WithNullItems_ThrowsArgumentNullException();
	void Command_WithEmptyItems_ThrowsArgumentException();
	void Command_WithValidItems_ReturnsValidCommand();
	void Items_ShouldBeImmutable();
}