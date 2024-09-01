using Application.Commands.Receipt;
using Application.Interfaces.Repositories;
using Moq;

namespace Application.Tests.Commands.Receipt;

public class DeleteReceiptCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsTrueAndCallsDeleteAndSaveChanges()
	{
		Mock<IReceiptRepository> mockRepository = new();
		DeleteReceiptCommandHandler handler = new(mockRepository.Object);

		List<Guid> inputReceiptIds =
		[
			Guid.NewGuid(),
			Guid.NewGuid()
		];

		DeleteReceiptCommand command = new(inputReceiptIds);

		await handler.Handle(command, CancellationToken.None);

		mockRepository.Verify(r => r.DeleteAsync(It.Is<List<Guid>>(ids =>
			ids.Count() == inputReceiptIds.Count &&
			ids.All(id => inputReceiptIds.Any(i => id == i))),
			It.IsAny<CancellationToken>()), Times.Once);

		mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}