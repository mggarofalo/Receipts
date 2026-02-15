using Application.Commands.ReceiptItem.Delete;
using Application.Interfaces.Services;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.ReceiptItem;

public class DeleteReceiptItemCommandHandlerTests
{
	[Fact]
	public async Task DeleteReceiptItemCommandHandler_WithValidCommand_ReturnsTrueAndCallsDeleteAndSaveChanges()
	{
		Mock<IReceiptItemService> mockService = new();
		DeleteReceiptItemCommandHandler handler = new(mockService.Object);

		List<Guid> input = [.. ReceiptItemGenerator.GenerateList(2).Select(ri => ri.Id)];

		DeleteReceiptItemCommand command = new(input);
		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);
	}
}