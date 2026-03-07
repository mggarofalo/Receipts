using API.Controllers.Core;
using API.Generated.Dtos;
using API.Mapping.Core;
using API.Services;
using Application.Commands.Account.Create;
using Application.Commands.Account.Delete;
using Application.Commands.Account.Restore;
using Application.Commands.Account.Update;
using Application.Models;
using Application.Queries.Core.Account;
using Domain.Core;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
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
	private readonly Mock<IEntityChangeNotifier> _notifierMock;
	private readonly AccountsController _controller;

	public AccountsControllerTests()
	{
		_mediatorMock = new Mock<IMediator>();
		_mapper = new AccountMapper();
		_loggerMock = ControllerTestHelpers.GetLoggerMock<AccountsController>();
		_notifierMock = new Mock<IEntityChangeNotifier>();
		_controller = new AccountsController(_mediatorMock.Object, _mapper, _loggerMock.Object, _notifierMock.Object);
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
		Results<Ok<AccountResponse>, NotFound> result = await _controller.GetAccountById(account.Id);

		// Assert
		Ok<AccountResponse> okResult = Assert.IsType<Ok<AccountResponse>>(result.Result);
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
		Results<Ok<AccountResponse>, NotFound> result = await _controller.GetAccountById(missingAccountId);

		// Assert
		Assert.IsType<NotFound>(result.Result);
	}

	[Fact]
	public async Task GetAccountById_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		Guid id = AccountGenerator.Generate().Id;

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAccountByIdQuery>(q => q.Id == id),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.GetAccountById(id);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task GetAllAccounts_ReturnsOkResult_WithListOfAccounts()
	{
		// Arrange
		List<Account> accounts = AccountGenerator.GenerateList(2);
		List<AccountResponse> expectedReturn = [.. accounts.Select(_mapper.ToResponse)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAllAccountsQuery>(q => q.Offset == 0 && q.Limit == 50),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(new PagedResult<Account>(accounts, accounts.Count, 0, 50));

		// Act
		Results<Ok<AccountListResponse>, BadRequest<string>> rawResult = await _controller.GetAllAccounts(0, 50, null, null);

		// Assert
		Ok<AccountListResponse> result = Assert.IsType<Ok<AccountListResponse>>(rawResult.Result);
		AccountListResponse actualReturn = result.Value!;

		actualReturn.Data.Should().BeEquivalentTo(expectedReturn);
		actualReturn.Total.Should().Be(accounts.Count);
		actualReturn.Offset.Should().Be(0);
		actualReturn.Limit.Should().Be(50);
	}

	[Theory]
	[InlineData(-1, 50)]
	[InlineData(-100, 50)]
	public async Task GetAllAccounts_ReturnsBadRequest_WhenOffsetIsNegative(int offset, int limit)
	{
		// Act
		Results<Ok<AccountListResponse>, BadRequest<string>> result = await _controller.GetAllAccounts(offset, limit, null, null);

		// Assert
		BadRequest<string> badRequestResult = Assert.IsType<BadRequest<string>>(result.Result);
		badRequestResult.Value.Should().Be("offset must be >= 0");
	}

	[Theory]
	[InlineData(0, 0)]
	[InlineData(0, -1)]
	[InlineData(0, 501)]
	public async Task GetAllAccounts_ReturnsBadRequest_WhenLimitIsOutOfRange(int offset, int limit)
	{
		// Act
		Results<Ok<AccountListResponse>, BadRequest<string>> result = await _controller.GetAllAccounts(offset, limit, null, null);

		// Assert
		BadRequest<string> badRequestResult = Assert.IsType<BadRequest<string>>(result.Result);
		badRequestResult.Value.Should().Be("limit must be between 1 and 500");
	}

	[Theory]
	[InlineData(-1, 50)]
	[InlineData(-100, 50)]
	public async Task GetDeletedAccounts_ReturnsBadRequest_WhenOffsetIsNegative(int offset, int limit)
	{
		// Act
		Results<Ok<AccountListResponse>, BadRequest<string>> result = await _controller.GetDeletedAccounts(offset, limit, null, null);

		// Assert
		BadRequest<string> badRequestResult = Assert.IsType<BadRequest<string>>(result.Result);
		badRequestResult.Value.Should().Be("offset must be >= 0");
	}

	[Theory]
	[InlineData(0, 0)]
	[InlineData(0, -1)]
	[InlineData(0, 501)]
	public async Task GetDeletedAccounts_ReturnsBadRequest_WhenLimitIsOutOfRange(int offset, int limit)
	{
		// Act
		Results<Ok<AccountListResponse>, BadRequest<string>> result = await _controller.GetDeletedAccounts(offset, limit, null, null);

		// Assert
		BadRequest<string> badRequestResult = Assert.IsType<BadRequest<string>>(result.Result);
		badRequestResult.Value.Should().Be("limit must be between 1 and 500");
	}

	[Fact]
	public async Task GetAllAccounts_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAllAccountsQuery>(q => q.Offset == 0 && q.Limit == 50),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.GetAllAccounts(0, 50, null, null);

		// Assert
		await act.Should().ThrowAsync<Exception>();
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
		Ok<AccountResponse> result = await _controller.CreateAccount(controllerInput);

		// Assert
		AccountResponse actualReturn = result.Value!;

		actualReturn.Should().BeEquivalentTo(expectedReturn);
	}

	[Fact]
	public async Task CreateAccount_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		CreateAccountRequest controllerInput = AccountDtoGenerator.GenerateCreateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateAccountCommand>(c => c.Accounts.Count == 1),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.CreateAccount(controllerInput);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task CreateAccounts_ReturnsOkResult_WithCreatedAccounts()
	{
		// Arrange
		List<Account> accounts = AccountGenerator.GenerateList(2);
		List<AccountResponse> expectedReturn = [.. accounts.Select(_mapper.ToResponse)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateAccountCommand>(c => c.Accounts.Count == accounts.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(accounts);

		List<CreateAccountRequest> controllerInput = AccountDtoGenerator.GenerateCreateRequestList(2);

		// Act
		Ok<List<AccountResponse>> result = await _controller.CreateAccounts(controllerInput);

		// Assert
		List<AccountResponse> actualReturn = result.Value!;

		actualReturn.Should().BeEquivalentTo(expectedReturn);
	}

	[Fact]
	public async Task CreateAccounts_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		List<CreateAccountRequest> controllerInput = AccountDtoGenerator.GenerateCreateRequestList(2);
		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateAccountCommand>(c => c.Accounts.Count == controllerInput.Count),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.CreateAccounts(controllerInput);

		// Assert
		await act.Should().ThrowAsync<Exception>();
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
		Results<NoContent, NotFound> result = await _controller.UpdateAccount(controllerInput.Id, controllerInput);

		// Assert
		Assert.IsType<NoContent>(result.Result);
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
		Results<NoContent, NotFound> result = await _controller.UpdateAccount(controllerInput.Id, controllerInput);

		// Assert
		Assert.IsType<NotFound>(result.Result);
	}

	[Fact]
	public async Task UpdateAccount_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		UpdateAccountRequest controllerInput = AccountDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateAccountCommand>(c => c.Accounts.Count == 1),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.UpdateAccount(controllerInput.Id, controllerInput);

		// Assert
		await act.Should().ThrowAsync<Exception>();
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
		Results<NoContent, NotFound> result = await _controller.UpdateAccounts(controllerInput);

		// Assert
		Assert.IsType<NoContent>(result.Result);
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
		Results<NoContent, NotFound> result = await _controller.UpdateAccounts(controllerInput);

		// Assert
		Assert.IsType<NotFound>(result.Result);
	}

	[Fact]
	public async Task UpdateAccounts_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		List<UpdateAccountRequest> controllerInput = AccountDtoGenerator.GenerateUpdateRequestList(2);
		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateAccountCommand>(c => c.Accounts.Count == controllerInput.Count),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.UpdateAccounts(controllerInput);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task DeleteAccounts_ReturnsNoContent_WhenDeleteSucceeds()
	{
		// Arrange
		List<Guid> ids = [.. AccountGenerator.GenerateList(2).Select(a => a.Id)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteAccountCommand>(c => c.Ids.SequenceEqual(ids)),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		Results<NoContent, NotFound> result = await _controller.DeleteAccounts(ids);

		// Assert
		Assert.IsType<NoContent>(result.Result);
	}

	[Fact]
	public async Task DeleteAccounts_ReturnsNotFound_WhenSingleAccountDeleteFails()
	{
		// Arrange
		List<Guid> ids = [AccountGenerator.Generate().Id];

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteAccountCommand>(c => c.Ids.SequenceEqual(ids)),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		Results<NoContent, NotFound> result = await _controller.DeleteAccounts(ids);

		// Assert
		Assert.IsType<NotFound>(result.Result);
	}

	[Fact]
	public async Task DeleteAccounts_ReturnsNotFound_WhenMultipleAccountsDeleteFails()
	{
		// Arrange
		List<Guid> ids = [.. AccountGenerator.GenerateList(2).Select(a => a.Id)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteAccountCommand>(c => c.Ids.SequenceEqual(ids)),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		Results<NoContent, NotFound> result = await _controller.DeleteAccounts(ids);

		// Assert
		Assert.IsType<NotFound>(result.Result);
	}

	[Fact]
	public async Task DeleteAccounts_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		List<Guid> ids = [.. AccountGenerator.GenerateList(2).Select(a => a.Id)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteAccountCommand>(c => c.Ids.SequenceEqual(ids)),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.DeleteAccounts(ids);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task RestoreAccount_ReturnsNoContent_WhenSuccessful()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		_mediatorMock.Setup(m => m.Send(
			It.Is<RestoreAccountCommand>(c => c.Id == id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		Results<NoContent, NotFound> result = await _controller.RestoreAccount(id);

		// Assert
		Assert.IsType<NoContent>(result.Result);
	}

	[Fact]
	public async Task RestoreAccount_ReturnsNotFound_WhenEntityDoesNotExist()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		_mediatorMock.Setup(m => m.Send(
			It.Is<RestoreAccountCommand>(c => c.Id == id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		Results<NoContent, NotFound> result = await _controller.RestoreAccount(id);

		// Assert
		Assert.IsType<NotFound>(result.Result);
	}

	[Fact]
	public async Task RestoreAccount_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		_mediatorMock.Setup(m => m.Send(
			It.Is<RestoreAccountCommand>(c => c.Id == id),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.RestoreAccount(id);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}
}
