using Application.Commands.ReceiptItem;
using Application.Interfaces.Repositories;
using Moq;

namespace Application.Tests.Commands.ReceiptItem;

public class DeleteReceiptItemCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsTrueAndCallsDeleteAndSaveChanges()
	{
		Mock<IReceiptItemRepository> mockRepository = new();
		DeleteReceiptItemCommandHandler handler = new(mockRepository.Object);

		List<Guid> inputReceiptItemIds =
		[
			Guid.NewGuid(),
			Guid.NewGuid()
		];

		DeleteReceiptItemCommand command = new(inputReceiptItemIds);

		await handler.Handle(command, CancellationToken.None);

		mockRepository.Verify(r => r.DeleteAsync(It.Is<List<Guid>>(ids =>
			ids.Count() == inputReceiptItemIds.Count &&
			ids.All(id => inputReceiptItemIds.Any(i => id == i))),
			It.IsAny<CancellationToken>()), Times.Once);

		mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}