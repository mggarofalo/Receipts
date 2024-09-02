using Application.Commands.Account;
using Application.Interfaces.Repositories;
using Moq;

namespace Application.Tests.Commands.Account;

public class DeleteAccountCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsTrueAndCallsDeleteAndSaveChanges()
	{
		Mock<IAccountRepository> mockRepository = new();
		DeleteAccountCommandHandler handler = new(mockRepository.Object);

		List<Guid> inputAccountIds =
		[
			Guid.NewGuid(),
			Guid.NewGuid()
		];

		DeleteAccountCommand command = new(inputAccountIds);

		await handler.Handle(command, CancellationToken.None);

		mockRepository.Verify(r => r.DeleteAsync(It.Is<List<Guid>>(ids =>
			ids.Count() == inputAccountIds.Count &&
			ids.All(id => inputAccountIds.Any(i => id == i))),
			It.IsAny<CancellationToken>()), Times.Once);

		mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}