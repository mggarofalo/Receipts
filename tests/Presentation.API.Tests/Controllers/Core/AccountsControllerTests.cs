using API.Controllers.Core;
using API.Generated.Dtos;
using API.Mapping.Core;
using Application.Commands.Account.Create;
using Application.Commands.Account.Delete;
using Application.Commands.Account.Update;
using Application.Queries.Core.Account;
using Domain.Core;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SampleData.Domain.Core;
using SampleData.Dtos.Core;

namespace Presentation.API.Tests.Controllers.Core;

public class AccountsControllerTests
{
	private readonly AccountMapper _mapper;
	private readonly Mock<IMediator> _mediatorMock;
	private readonly Mock<ILogger<AccountsController>> _loggerMock;
	private readonly AccountsController _controller;

	public AccountsControllerTests()
	{
		_mediatorMock = new Mock<IMediator>();
		_mapper = new AccountMapper();
		_loggerMock = ControllerTestHelpers.GetLoggerMock<AccountsController>();
		_controller = new AccountsController(_mediatorMock.Object, _mapper, _loggerMock.Object);
	}

	[Fact]
	public async Task GetAccountById_ReturnsOkResult_WhenAccountExists()
	{
		// Arrange
		Account account = AccountGenerator.Generate();
		AccountResponse expectedReturn = _mapper.ToResponse(account);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAccountByIdQuery>(q => q.Id == account.Id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(account);

		// Act
		ActionResult<AccountResponse> result = await _controller.GetAccountById(account.Id);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		AccountResponse actualReturn = Assert.IsType<AccountResponse>(okResult.Value);
		actualReturn.Should().BeEquivalentTo(expectedReturn);
	}

	[Fact]
	public async Task GetAccountById_ReturnsNotFound_WhenAccountDoesNotExist()
	{
		// Arrange
		Guid missingAccountId = Guid.NewGuid();

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAccountByIdQuery>(q => q.Id == missingAccountId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync((Account?)null);

		// Act
		ActionResult<AccountResponse> result = await _controller.GetAccountById(missingAccountId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task GetAccountById_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		Guid id = AccountGenerator.Generate().Id;

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAccountByIdQuery>(q => q.Id == id),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<AccountResponse> result = await _controller.GetAccountById(id);

		// Assert
		Assert.IsType<ObjectResult>(result.Result);
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task GetAllAccounts_ReturnsOkResult_WithListOfAccounts()
	{
		// Arrange
		List<Account> accounts = AccountGenerator.GenerateList(2);
		List<AccountResponse> expectedReturn = accounts.Select(_mapper.ToResponse).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAllAccountsQuery>(q => true),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(accounts);

		// Act
		ActionResult<List<AccountResponse>> result = await _controller.GetAllAccounts();

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<AccountResponse> actualReturn = Assert.IsType<List<AccountResponse>>(okResult.Value);

		actualReturn.Should().BeEquivalentTo(expectedReturn);
	}

	[Fact]
	public async Task GetAllAccounts_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAllAccountsQuery>(q => true),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<List<AccountResponse>> result = await _controller.GetAllAccounts();

		// Assert
		Assert.IsType<ObjectResult>(result.Result);
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task CreateAccount_ReturnsOkResult_WithCreatedAccount()
	{
		// Arrange
		Account account = AccountGenerator.Generate();
		AccountResponse expectedReturn = _mapper.ToResponse(account);

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateAccountCommand>(c => c.Accounts.Count == 1),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync([account]);

		CreateAccountRequest controllerInput = AccountDtoGenerator.GenerateCreateRequest();

		// Act
		ActionResult<AccountResponse> result = await _controller.CreateAccount(controllerInput);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		AccountResponse actualReturn = Assert.IsType<AccountResponse>(okResult.Value);

		actualReturn.Should().BeEquivalentTo(expectedReturn);
	}

	[Fact]
	public async Task CreateAccount_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		CreateAccountRequest controllerInput = AccountDtoGenerator.GenerateCreateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateAccountCommand>(c => c.Accounts.Count == 1),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<AccountResponse> result = await _controller.CreateAccount(controllerInput);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task CreateAccounts_ReturnsOkResult_WithCreatedAccounts()
	{
		// Arrange
		List<Account> accounts = AccountGenerator.GenerateList(2);
		List<AccountResponse> expectedReturn = accounts.Select(_mapper.ToResponse).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateAccountCommand>(c => c.Accounts.Count == accounts.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(accounts);

		List<CreateAccountRequest> controllerInput = AccountDtoGenerator.GenerateCreateRequestList(2);

		// Act
		ActionResult<List<AccountResponse>> result = await _controller.CreateAccounts(controllerInput);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<AccountResponse> actualReturn = Assert.IsType<List<AccountResponse>>(okResult.Value);

		actualReturn.Should().BeEquivalentTo(expectedReturn);
	}

	[Fact]
	public async Task CreateAccounts_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		List<CreateAccountRequest> controllerInput = AccountDtoGenerator.GenerateCreateRequestList(2);
		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateAccountCommand>(c => c.Accounts.Count == controllerInput.Count),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<List<AccountResponse>> result = await _controller.CreateAccounts(controllerInput);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task UpdateAccount_ReturnsNoContent_WhenUpdateSucceeds()
	{
		// Arrange
		UpdateAccountRequest controllerInput = AccountDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateAccountCommand>(c => c.Accounts.Count == 1),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		ActionResult<bool> result = await _controller.UpdateAccount(controllerInput.Id, controllerInput);

		// Assert
		NoContentResult noContentResult = Assert.IsType<NoContentResult>(result.Result);
		Assert.Equal(204, noContentResult.StatusCode);
	}

	[Fact]
	public async Task UpdateAccount_ReturnsNotFound_WhenUpdateFails()
	{
		// Arrange
		UpdateAccountRequest controllerInput = AccountDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateAccountCommand>(c => c.Accounts.Count == 1),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.UpdateAccount(controllerInput.Id, controllerInput);

		// Assert
		NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
		Assert.Equal(404, notFoundResult.StatusCode);
	}

	[Fact]
	public async Task UpdateAccount_ReturnsInternalServerError_WhenExceptionThrown()
	{
		// Arrange
		UpdateAccountRequest controllerInput = AccountDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateAccountCommand>(c => c.Accounts.Count == 1),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<bool> result = await _controller.UpdateAccount(controllerInput.Id, controllerInput);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task UpdateAccounts_ReturnsNoContent_WhenUpdateSucceeds()
	{
		// Arrange
		List<UpdateAccountRequest> controllerInput = AccountDtoGenerator.GenerateUpdateRequestList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateAccountCommand>(c => c.Accounts.Count == controllerInput.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		ActionResult<bool> result = await _controller.UpdateAccounts(controllerInput);

		// Assert
		NoContentResult noContentResult = Assert.IsType<NoContentResult>(result.Result);
		Assert.Equal(204, noContentResult.StatusCode);
	}

	[Fact]
	public async Task UpdateAccounts_ReturnsNotFound_WhenUpdateFails()
	{
		// Arrange
		List<UpdateAccountRequest> controllerInput = AccountDtoGenerator.GenerateUpdateRequestList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateAccountCommand>(c => c.Accounts.Count == controllerInput.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.UpdateAccounts(controllerInput);

		// Assert
		NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
		Assert.Equal(404, notFoundResult.StatusCode);
	}

	[Fact]
	public async Task UpdateAccounts_ReturnsInternalServerError_WhenExceptionThrown()
	{
		// Arrange
		List<UpdateAccountRequest> controllerInput = AccountDtoGenerator.GenerateUpdateRequestList(2);
		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateAccountCommand>(c => c.Accounts.Count == controllerInput.Count),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<bool> result = await _controller.UpdateAccounts(controllerInput);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task DeleteAccounts_ReturnsNoContent_WhenDeleteSucceeds()
	{
		// Arrange
		List<Guid> ids = AccountGenerator.GenerateList(2).Select(a => a.Id).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteAccountCommand>(c => c.Ids.SequenceEqual(ids)),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		ActionResult<bool> result = await _controller.DeleteAccounts(ids);

		// Assert
		NoContentResult noContentResult = Assert.IsType<NoContentResult>(result.Result);
		Assert.Equal(204, noContentResult.StatusCode);
	}

	[Fact]
	public async Task DeleteAccounts_ReturnsNotFound_WhenSingleAccountDeleteFails()
	{
		// Arrange
		List<Guid> ids = [AccountGenerator.Generate().Id];

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<DeleteAccountCommand>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.DeleteAccounts(ids);

		// Assert
		NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
		Assert.Equal(404, notFoundResult.StatusCode);
	}

	[Fact]
	public async Task DeleteAccounts_ReturnsNotFound_WhenMultipleAccountsDeleteFails()
	{
		// Arrange
		List<Guid> ids = AccountGenerator.GenerateList(2).Select(a => a.Id).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<DeleteAccountCommand>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.DeleteAccounts(ids);

		// Assert
		NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
		Assert.Equal(404, notFoundResult.StatusCode);
	}

	[Fact]
	public async Task DeleteAccounts_ReturnsInternalServerError_WhenExceptionThrown()
	{
		// Arrange
		List<Guid> ids = AccountGenerator.GenerateList(2).Select(a => a.Id).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<DeleteAccountCommand>(),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<bool> result = await _controller.DeleteAccounts(ids);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}
}
