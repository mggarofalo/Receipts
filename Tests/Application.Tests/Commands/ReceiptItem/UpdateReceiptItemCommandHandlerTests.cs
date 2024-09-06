using Application.Commands.ReceiptItem;
using Application.Interfaces.Repositories;
using SampleData.Domain.Core;
using Moq;

namespace Application.Tests.Commands.ReceiptItem;

public class UpdateReceiptItemCommandHandlerTests
{
	[Fact]
	public async Task UpdateReceiptItemCommandHandler_WithValidCommand_ReturnsTrueAndCallsUpdateAndSaveChanges()
	{
		Mock<IReceiptItemRepository> mockRepository = new();
		UpdateReceiptItemCommandHandler handler = new(mockRepository.Object);

		List<Domain.Core.ReceiptItem> input = ReceiptItemGenerator.GenerateList(2);

		mockRepository.Setup(r => r
			.UpdateAsync(It.IsAny<List<Domain.Core.ReceiptItem>>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		UpdateReceiptItemCommand command = new(input);
		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);
	}
}