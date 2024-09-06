using Application.Commands.ReceiptItem;
using Application.Interfaces.Repositories;
using SampleData.Domain.Core;
using Moq;

namespace Application.Tests.Commands.ReceiptItem;

public class DeleteReceiptItemCommandHandlerTests
{
	[Fact]
	public async Task DeleteReceiptItemCommandHandler_WithValidCommand_ReturnsTrueAndCallsDeleteAndSaveChanges()
	{
		Mock<IReceiptItemRepository> mockRepository = new();
		DeleteReceiptItemCommandHandler handler = new(mockRepository.Object);

		List<Guid> input = ReceiptItemGenerator.GenerateList(2).Select(ri => ri.Id!.Value).ToList();

		DeleteReceiptItemCommand command = new(input);
		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);
	}
}