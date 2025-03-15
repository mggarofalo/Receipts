using Application.Commands.Transaction.Update;
using SampleData.Domain.Core;
using Moq;
using Application.Interfaces.Services;

namespace Application.Tests.Commands.Transaction;

public class UpdateTransactionCommandHandlerTests
{
	[Fact]
	public async Task UpdateTransactionCommandHandler_WithValidCommand_ReturnsTrueAndCallsUpdateAndSaveChanges()
	{
		Mock<ITransactionService> mockService = new();
		UpdateTransactionCommandHandler handler = new(mockService.Object);

		List<Domain.Core.Transaction> input = TransactionGenerator.GenerateList(2);

		mockService.Setup(r => r
			.UpdateAsync(It.IsAny<List<Domain.Core.Transaction>>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		UpdateTransactionCommand command = new(input, Guid.NewGuid(), Guid.NewGuid());
		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);
	}
}