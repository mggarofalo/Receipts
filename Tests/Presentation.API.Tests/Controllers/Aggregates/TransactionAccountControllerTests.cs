using API.Controllers.Aggregates;
using API.Mapping.Aggregates;
using API.Mapping.Core;
using Application.Queries.Aggregates.TransactionAccounts;
using AutoMapper;
using Domain.Aggregates;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SampleData.Domain.Aggregates;
using SampleData.Domain.Core;
using Shared.ViewModels.Aggregates;

namespace Presentation.API.Tests.Controllers.Aggregates;

public class TransactionAccountControllerTests
{
	private readonly IMapper _mapper;
	private readonly Mock<IMediator> _mediatorMock;
	private readonly Mock<IMapper> _mapperMock;
	private readonly Mock<ILogger<TransactionAccountController>> _loggerMock;
	private readonly TransactionAccountController _controller;

	public TransactionAccountControllerTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<TransactionAccountMappingProfile>();
			cfg.AddProfile<TransactionMappingProfile>();
			cfg.AddProfile<AccountMappingProfile>();
		});

		_mapper = configuration.CreateMapper();
		_mediatorMock = new Mock<IMediator>();
		_mapperMock = ControllerTestHelpers.GetMapperMock<TransactionAccount, TransactionAccountVM>(_mapper);
		_loggerMock = ControllerTestHelpers.GetLoggerMock<TransactionAccountController>();
		_controller = new TransactionAccountController(_mediatorMock.Object, _mapperMock.Object, _loggerMock.Object);
	}

	[Fact]
	public async Task GetTransactionAccountByTransactionId_ReturnsOkResult_WhenTransactionExists()
	{
		// Arrange
		TransactionAccount transactionAccount = TransactionAccountGenerator.Generate();
		TransactionAccountVM expectedReturn = _mapper.Map<TransactionAccountVM>(transactionAccount);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionAccountByTransactionIdQuery>(q => q.TransactionId == transactionAccount.Transaction.Id!.Value),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(transactionAccount);

		// Act
		ActionResult<TransactionAccountVM> result = await _controller.GetTransactionAccountByTransactionId(transactionAccount.Transaction.Id!.Value);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		TransactionAccountVM actualReturn = Assert.IsType<TransactionAccountVM>(okResult.Value);

		Assert.Equal(expectedReturn, actualReturn);
	}

	[Fact]
	public async Task GetTransactionAccountByTransactionId_ReturnsNotFound_WhenTransactionDoesNotExist()
	{
		// Arrange
		Guid missingTransactionId = Guid.NewGuid();

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionAccountByTransactionIdQuery>(q => q.TransactionId == missingTransactionId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync((TransactionAccount?)null);

		// Act
		ActionResult<TransactionAccountVM> result = await _controller.GetTransactionAccountByTransactionId(missingTransactionId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task GetTransactionAccountByTransactionId_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		Guid transactionId = TransactionAccountGenerator.Generate().Transaction.Id!.Value;

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionAccountByTransactionIdQuery>(q => q.TransactionId == transactionId),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<TransactionAccountVM> result = await _controller.GetTransactionAccountByTransactionId(transactionId);

		// Assert
		Assert.IsType<ObjectResult>(result.Result);
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task GetTransactionAccountsByReceiptId_ReturnsOkResult_WhenReceiptExists()
	{
		// Arrange
		Guid receiptId = ReceiptGenerator.Generate().Id!.Value;
		TransactionAccount transactionAccount = TransactionAccountGenerator.Generate();
		TransactionAccountVM expectedReturn = _mapper.Map<TransactionAccountVM>(transactionAccount);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionAccountsByReceiptIdQuery>(q => q.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync([transactionAccount]);

		// Act
		ActionResult<List<TransactionAccountVM>> result = await _controller.GetTransactionAccountsByReceiptId(receiptId);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<TransactionAccountVM> actualReturn = Assert.IsType<List<TransactionAccountVM>>(okResult.Value);

		Assert.Equal(expectedReturn, actualReturn[0]);
	}

	[Fact]
	public async Task GetTransactionAccountsByReceiptId_ReturnsNotFound_WhenReceiptDoesNotExist()
	{
		// Arrange
		Guid missingReceiptId = Guid.NewGuid();

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionAccountsByReceiptIdQuery>(q => q.ReceiptId == missingReceiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync((List<TransactionAccount>?)null);

		// Act
		ActionResult<List<TransactionAccountVM>> result = await _controller.GetTransactionAccountsByReceiptId(missingReceiptId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task GetTransactionAccountsByReceiptId_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		Guid receiptId = ReceiptGenerator.Generate().Id!.Value;

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionAccountsByReceiptIdQuery>(q => q.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<List<TransactionAccountVM>> result = await _controller.GetTransactionAccountsByReceiptId(receiptId);

		// Assert
		Assert.IsType<ObjectResult>(result.Result);
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}
}
