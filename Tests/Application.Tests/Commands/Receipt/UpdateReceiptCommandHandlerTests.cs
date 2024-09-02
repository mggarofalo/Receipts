using Application.Commands.Receipt;
using Application.Interfaces.Repositories;
using SampleData.Domain.Core;
using Moq;

namespace Application.Tests.Commands.Receipt;

public class UpdateReceiptCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsTrueAndCallsUpdateAndSaveChanges()
	{
		Mock<IReceiptRepository> mockRepository = new();
		UpdateReceiptCommandHandler handler = new(mockRepository.Object);

		List<Domain.Core.Receipt> input = ReceiptGenerator.GenerateList(2);

		mockRepository.Setup(r => r
			.UpdateAsync(It.IsAny<List<Domain.Core.Receipt>>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		UpdateReceiptCommand command = new(input);
		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);

		mockRepository.Verify(r => r.UpdateAsync(It.IsAny<List<Domain.Core.Receipt>>(), It.IsAny<CancellationToken>()), Times.Once);
		mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}