using API.Controllers.Aggregates;
using API.Generated.Dtos;
using API.Mapping.Aggregates;
using API.Mapping.Core;
using Application.Queries.Aggregates.TransactionAccounts;
using Domain.Aggregates;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SampleData.Domain.Aggregates;
using SampleData.Domain.Core;

namespace Presentation.API.Tests.Controllers.Aggregates;

public class TransactionAccountControllerTests
{
	private readonly TransactionAccountMapper _mapper;
	private readonly Mock<IMediator> _mediatorMock;
	private readonly Mock<ILogger<TransactionAccountController>> _loggerMock;
	private readonly TransactionAccountController _controller;

	public TransactionAccountControllerTests()
	{
		_mediatorMock = new Mock<IMediator>();
		_mapper = new TransactionAccountMapper();
		_loggerMock = ControllerTestHelpers.GetLoggerMock<TransactionAccountController>();
		_controller = new TransactionAccountController(_mediatorMock.Object, _mapper, _loggerMock.Object);
	}

	[Fact]
	public async Task GetTransactionAccountByTransactionId_ReturnsOkResult_WhenTransactionExists()
	{
		// Arrange
		TransactionAccount transactionAccount = TransactionAccountGenerator.Generate();
		TransactionAccountResponse expectedReturn = _mapper.ToResponse(transactionAccount);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionAccountByTransactionIdQuery>(q => q.TransactionId == transactionAccount.Transaction.Id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(transactionAccount);

		// Act
		ActionResult<TransactionAccountResponse> result = await _controller.GetTransactionAccountByTransactionId(transactionAccount.Transaction.Id);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		TransactionAccountResponse actualReturn = Assert.IsType<TransactionAccountResponse>(okResult.Value);

		actualReturn.Should().BeEquivalentTo(expectedReturn);
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
		ActionResult<TransactionAccountResponse> result = await _controller.GetTransactionAccountByTransactionId(missingTransactionId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task GetTransactionAccountByTransactionId_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		Guid transactionId = TransactionAccountGenerator.Generate().Transaction.Id;

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionAccountByTransactionIdQuery>(q => q.TransactionId == transactionId),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<TransactionAccountResponse> result = await _controller.GetTransactionAccountByTransactionId(transactionId);

		// Assert
		Assert.IsType<ObjectResult>(result.Result);
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task GetTransactionAccountsByReceiptId_ReturnsOkResult_WhenReceiptExists()
	{
		// Arrange
		Guid receiptId = ReceiptGenerator.Generate().Id;
		TransactionAccount transactionAccount = TransactionAccountGenerator.Generate();
		TransactionAccountResponse expectedReturn = _mapper.ToResponse(transactionAccount);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionAccountsByReceiptIdQuery>(q => q.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync([transactionAccount]);

		// Act
		ActionResult<List<TransactionAccountResponse>> result = await _controller.GetTransactionAccountsByReceiptId(receiptId);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<TransactionAccountResponse> actualReturn = Assert.IsType<List<TransactionAccountResponse>>(okResult.Value);

		actualReturn[0].Should().BeEquivalentTo(expectedReturn);
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
		ActionResult<List<TransactionAccountResponse>> result = await _controller.GetTransactionAccountsByReceiptId(missingReceiptId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task GetTransactionAccountsByReceiptId_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		Guid receiptId = ReceiptGenerator.Generate().Id;

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionAccountsByReceiptIdQuery>(q => q.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<List<TransactionAccountResponse>> result = await _controller.GetTransactionAccountsByReceiptId(receiptId);

		// Assert
		Assert.IsType<ObjectResult>(result.Result);
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}
}
