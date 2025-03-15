using Application.Commands.Account.Delete;
using SampleData.Domain.Core;
using Moq;
using Application.Interfaces.Services;

namespace Application.Tests.Commands.Account;

public class DeleteAccountCommandHandlerTests
{
	[Fact]
	public async Task DeleteAccountCommandHandler_WithValidCommand_ReturnsTrueAndCallsDeleteAndSaveChanges()
	{
		Mock<IAccountService> mockService = new();
		DeleteAccountCommandHandler handler = new(mockService.Object);

		List<Guid> input = AccountGenerator.GenerateList(2).Select(a => a.Id!.Value).ToList();

		DeleteAccountCommand command = new(input);
		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);
	}
}