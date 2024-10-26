using Application.Commands.ReceiptItem;
using SampleData.Domain.Core;
using Moq;
using Application.Interfaces.Services;

namespace Application.Tests.Commands.ReceiptItem;

public class UpdateReceiptItemCommandHandlerTests
{
	[Fact]
	public async Task UpdateReceiptItemCommandHandler_WithValidCommand_ReturnsTrueAndCallsUpdateAndSaveChanges()
	{
		Mock<IReceiptItemService> mockService = new();
		UpdateReceiptItemCommandHandler handler = new(mockService.Object);

		List<Domain.Core.ReceiptItem> input = ReceiptItemGenerator.GenerateList(2);

		mockService.Setup(r => r
			.UpdateAsync(It.IsAny<List<Domain.Core.ReceiptItem>>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		UpdateReceiptItemCommand command = new(input, Guid.NewGuid());
		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);
	}
}