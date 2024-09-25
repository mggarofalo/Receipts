using Application.Commands.Account;
using Application.Interfaces.Repositories;
using SampleData.Domain.Core;
using Moq;

namespace Application.Tests.Commands.Account;

public class DeleteAccountCommandHandlerTests
{
	[Fact]
	public async Task DeleteAccountCommandHandler_WithValidCommand_ReturnsTrueAndCallsDeleteAndSaveChanges()
	{
		Mock<IAccountRepository> mockRepository = new();
		DeleteAccountCommandHandler handler = new(mockRepository.Object);

		List<Guid> input = AccountGenerator.GenerateList(2).Select(a => a.Id!.Value).ToList();

		DeleteAccountCommand command = new(input);
		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);
	}
}