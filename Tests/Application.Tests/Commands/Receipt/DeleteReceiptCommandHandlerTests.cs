using Application.Commands.Receipt;
using Application.Interfaces.Repositories;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Receipt;

public class DeleteReceiptCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsTrueAndCallsDeleteAndSaveChanges()
	{
		Mock<IReceiptRepository> mockRepository = new();
		DeleteReceiptCommandHandler handler = new(mockRepository.Object);

		List<Guid> input = ReceiptGenerator.GenerateList(2).Select(r => r.Id!.Value).ToList();

		DeleteReceiptCommand command = new(input);
		await handler.Handle(command, CancellationToken.None);

		mockRepository.Verify(r => r.DeleteAsync(It.Is<List<Guid>>(ids =>
			ids.Count() == input.Count &&
			ids.All(id => input.Any(i => id == i))),
			It.IsAny<CancellationToken>()), Times.Once);

		mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}