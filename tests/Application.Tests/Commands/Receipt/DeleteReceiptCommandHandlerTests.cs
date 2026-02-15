using Application.Commands.Receipt.Delete;
using Application.Interfaces.Services;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Receipt;

public class DeleteReceiptCommandHandlerTests
{
	[Fact]
	public async Task DeleteReceiptCommandHandler_WithValidCommand_ReturnsTrueAndCallsDeleteAndSaveChanges()
	{
		Mock<IReceiptService> mockService = new();
		DeleteReceiptCommandHandler handler = new(mockService.Object);

		List<Guid> input = [.. ReceiptGenerator.GenerateList(2).Select(r => r.Id)];

		DeleteReceiptCommand command = new(input);
		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);
	}
}