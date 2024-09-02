using Application.Commands.Account;
using Application.Interfaces.Repositories;
using SampleData.Domain.Core;
using Moq;

namespace Application.Tests.Commands.Account;

public class DeleteAccountCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsTrueAndCallsDeleteAndSaveChanges()
	{
		Mock<IAccountRepository> mockRepository = new();
		DeleteAccountCommandHandler handler = new(mockRepository.Object);

		List<Guid> input = AccountGenerator.GenerateList(2).Select(a => a.Id!.Value).ToList();

		DeleteAccountCommand command = new(input);
		await handler.Handle(command, CancellationToken.None);

		mockRepository.Verify(r => r.DeleteAsync(It.Is<List<Guid>>(ids =>
			ids.Count() == input.Count &&
			ids.All(id => input.Any(i => id == i))),
			It.IsAny<CancellationToken>()), Times.Once);

		mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}