using Application.Commands.ReceiptItem;
using Application.Interfaces.Repositories;
using SampleData.Domain.Core;
using Moq;

namespace Application.Tests.Commands.ReceiptItem;

public class DeleteReceiptItemCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsTrueAndCallsDeleteAndSaveChanges()
	{
		Mock<IReceiptItemRepository> mockRepository = new();
		DeleteReceiptItemCommandHandler handler = new(mockRepository.Object);

		List<Guid> input = ReceiptItemGenerator.GenerateList(2).Select(ri => ri.Id!.Value).ToList();

		DeleteReceiptItemCommand command = new(input);
		await handler.Handle(command, CancellationToken.None);

		mockRepository.Verify(r => r.DeleteAsync(It.Is<List<Guid>>(ids =>
			ids.Count() == input.Count &&
			ids.All(id => input.Any(i => id == i))),
			It.IsAny<CancellationToken>()), Times.Once);

		mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}