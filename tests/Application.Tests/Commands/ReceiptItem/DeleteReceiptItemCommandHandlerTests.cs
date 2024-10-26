using Application.Commands.ReceiptItem;
using SampleData.Domain.Core;
using Moq;
using Application.Interfaces.Services;

namespace Application.Tests.Commands.ReceiptItem;

public class DeleteReceiptItemCommandHandlerTests
{
	[Fact]
	public async Task DeleteReceiptItemCommandHandler_WithValidCommand_ReturnsTrueAndCallsDeleteAndSaveChanges()
	{
		Mock<IReceiptItemService> mockService = new();
		DeleteReceiptItemCommandHandler handler = new(mockService.Object);

		List<Guid> input = ReceiptItemGenerator.GenerateList(2).Select(ri => ri.Id!.Value).ToList();

		DeleteReceiptItemCommand command = new(input);
		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);
	}
}