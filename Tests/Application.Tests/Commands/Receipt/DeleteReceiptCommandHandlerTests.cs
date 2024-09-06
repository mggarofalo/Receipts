using Application.Commands.Receipt;
using Application.Interfaces.Repositories;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Receipt;

public class DeleteReceiptCommandHandlerTests
{
	[Fact]
	public async Task DeleteReceiptCommandHandler_WithValidCommand_ReturnsTrueAndCallsDeleteAndSaveChanges()
	{
		Mock<IReceiptRepository> mockRepository = new();
		DeleteReceiptCommandHandler handler = new(mockRepository.Object);

		List<Guid> input = ReceiptGenerator.GenerateList(2).Select(r => r.Id!.Value).ToList();

		DeleteReceiptCommand command = new(input);
		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);
	}
}