using Application.Commands.Account.Update;
using Application.Interfaces.Services;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Account;

public class UpdateAccountCommandHandlerTests
{
	[Fact]
	public async Task UpdateAccountCommandHandler_WithValidCommand_ReturnsTrueAndCallsUpdateAndSaveChanges()
	{
		Mock<IAccountService> mockService = new();
		UpdateAccountCommandHandler handler = new(mockService.Object);

		List<Domain.Core.Account> input = AccountGenerator.GenerateList(2);

		mockService.Setup(r => r
			.UpdateAsync(It.IsAny<List<Domain.Core.Account>>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		UpdateAccountCommand command = new(input);
		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);
	}
}