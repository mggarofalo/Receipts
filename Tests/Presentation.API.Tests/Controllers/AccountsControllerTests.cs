using API.Controllers;
using API.Mapping.Aggregates;
using API.Mapping.Core;
using Application.Commands.Account;
using Application.Queries.Core.Account;
using AutoMapper;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SampleData.Domain.Core;
using SampleData.ViewModels.Core;
using Shared.ViewModels.Core;

namespace Presentation.API.Tests.Controllers;

public class AccountsControllerTests
{
	private readonly IMapper _mapper;
	private readonly Mock<IMediator> _mediatorMock;
	private readonly Mock<IMapper> _mapperMock;
	private readonly Mock<ILogger<AccountsController>> _loggerMock;
	private readonly AccountsController _controller;

	public AccountsControllerTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<TripMappingProfile>();
			cfg.AddProfile<ReceiptWithItemsMappingProfile>();
			cfg.AddProfile<ReceiptMappingProfile>();
			cfg.AddProfile<ReceiptItemMappingProfile>();
			cfg.AddProfile<TransactionAccountMappingProfile>();
			cfg.AddProfile<TransactionMappingProfile>();
			cfg.AddProfile<AccountMappingProfile>();
		});

		_mapper = configuration.CreateMapper();

		_mediatorMock = new Mock<IMediator>();

		_mapperMock = new Mock<IMapper>();
		_mapperMock.Setup(m => m.Map<Account, AccountVM>(It.IsAny<Account>())).Returns<Account>(_mapper.Map<AccountVM>);
		_mapperMock.Setup(m => m.Map<AccountVM, Account>(It.IsAny<AccountVM>())).Returns<AccountVM>(_mapper.Map<Account>);

		_loggerMock = new Mock<ILogger<AccountsController>>();

		_controller = new AccountsController(_mediatorMock.Object, _mapperMock.Object, _loggerMock.Object);

		// TODO: Add verification of mapper and mediator calls to each test
	}

	[Fact]
	public async Task GetAccountById_ReturnsOkResult_WhenAccountExists()
	{
		// Arrange
		Account account = AccountGenerator.Generate();
		AccountVM expectedReturn = _mapper.Map<AccountVM>(account);

		_mediatorMock.Setup(m => m.Send(It.IsAny<GetAccountByIdQuery>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(account);

		// Act
		ActionResult<AccountVM> result = await _controller.GetAccountById(account.Id!.Value);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		AccountVM actualReturn = Assert.IsType<AccountVM>(okResult.Value);
		Assert.Equal(expectedReturn, actualReturn);
	}

	[Fact]
	public async Task GetAccountById_ReturnsNotFound_WhenAccountDoesNotExist()
	{
		// Arrange
		Guid missingAccountId = Guid.NewGuid();

		_mediatorMock.Setup(m => m.Send(It.IsAny<GetAccountByIdQuery>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((Account?)null);

		// Act
		ActionResult<AccountVM> result = await _controller.GetAccountById(missingAccountId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task GetAllAccounts_ReturnsOkResult_WithListOfAccounts()
	{
		// Arrange
		List<Account> accounts = AccountGenerator.GenerateList(2);
		List<AccountVM> expectedReturn = _mapper.Map<List<AccountVM>>(accounts);

		_mediatorMock.Setup(m => m.Send(It.IsAny<GetAllAccountsQuery>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(accounts);

		// Act
		ActionResult<List<AccountVM>> result = await _controller.GetAllAccounts();

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<AccountVM> actualReturn = Assert.IsType<List<AccountVM>>(okResult.Value);

		Assert.Equal(expectedReturn.Count, actualReturn.Count);
		Assert.Equal(expectedReturn, actualReturn);
	}

	[Fact]
	public async Task CreateAccount_ReturnsOkResult_WithCreatedAccounts()
	{
		// Arrange
		List<Account> accounts = AccountGenerator.GenerateList(2);
		List<AccountVM> expectedReturn = _mapper.Map<List<AccountVM>>(accounts);

		_mediatorMock.Setup(m => m.Send(It.IsAny<CreateAccountCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(accounts);

		List<AccountVM> models = _mapper.Map<List<AccountVM>>(accounts);
		models.ForEach(a => a.Id = null);

		// Act
		ActionResult<AccountVM> result = await _controller.CreateAccount(models);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<AccountVM> actualReturn = Assert.IsType<List<AccountVM>>(okResult.Value);

		Assert.Equal(expectedReturn.Count, actualReturn.Count);
		Assert.Equal(expectedReturn, actualReturn);
	}

	[Fact]
	public async Task UpdateAccounts_ReturnsNoContent_WhenUpdateSucceeds()
	{
		// Arrange
		List<AccountVM> models = AccountVMGenerator.GenerateList(2);
		List<Account> accounts = _mapper.Map<List<Account>>(models);

		_mediatorMock.Setup(m => m.Send(It.IsAny<UpdateAccountCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		ActionResult<bool> result = await _controller.UpdateAccounts(models);

		// Assert
		NoContentResult noContentResult = Assert.IsType<NoContentResult>(result.Result);
		Assert.Equal(204, noContentResult.StatusCode);
	}

	[Fact]
	public async Task UpdateAccounts_ReturnsNotFound_WhenUpdateFails()
	{
		// Arrange
		List<AccountVM> models = AccountVMGenerator.GenerateList(2);

		_mediatorMock.Setup(m => m.Send(It.IsAny<UpdateAccountCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.UpdateAccounts(models);

		// Assert
		NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
		Assert.Equal(404, notFoundResult.StatusCode);
	}

	[Fact]
	public async Task DeleteAccounts_ReturnsNoContent_WhenDeleteSucceeds()
	{
		// Arrange
		List<Guid> ids = AccountGenerator.GenerateList(2).Select(a => a.Id!.Value).ToList();

		_mediatorMock.Setup(m => m.Send(It.IsAny<DeleteAccountCommand>(), It.IsAny<CancellationToken>()))
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
		Guid id = AccountGenerator.Generate().Id!.Value;

		_mediatorMock.Setup(m => m.Send(It.IsAny<DeleteAccountCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.DeleteAccounts([id]);

		// Assert
		NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
		Assert.Equal(404, notFoundResult.StatusCode);
	}

	[Fact]
	public async Task DeleteAccounts_ReturnsNotFound_WhenMultipleAccountsDeleteFails()
	{
		// Arrange
		List<Guid> ids = AccountGenerator.GenerateList(2).Select(a => a.Id!.Value).ToList();

		_mediatorMock.Setup(m => m.Send(It.IsAny<DeleteAccountCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.DeleteAccounts(ids);

		// Assert
		NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
		Assert.Equal(404, notFoundResult.StatusCode);
	}
}