using API.Controllers.Core;
using API.Generated.Dtos;
using API.Mapping.Core;
using Application.Commands.Transaction.Create;
using Application.Commands.Transaction.Delete;
using Application.Commands.Transaction.Update;
using Application.Queries.Core.Transaction;
using Domain.Core;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SampleData.Domain.Core;
using SampleData.Dtos.Core;

namespace Presentation.API.Tests.Controllers.Core;

public class TransactionsControllerTests
{
	private readonly TransactionMapper _mapper;
	private readonly Mock<IMediator> _mediatorMock;
	private readonly Mock<ILogger<TransactionsController>> _loggerMock;
	private readonly TransactionsController _controller;

	public TransactionsControllerTests()
	{
		_mediatorMock = new Mock<IMediator>();
		_mapper = new TransactionMapper();
		_loggerMock = ControllerTestHelpers.GetLoggerMock<TransactionsController>();
		_controller = new TransactionsController(_mediatorMock.Object, _mapper, _loggerMock.Object);
	}

	[Fact]
	public async Task GetTransactionById_ReturnsOkResult_WithTransaction()
	{
		// Arrange
		Transaction mediatorReturn = TransactionGenerator.Generate();
		TransactionResponse expectedControllerReturn = _mapper.ToResponse(mediatorReturn);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionByIdQuery>(q => q.Id == mediatorReturn.Id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(mediatorReturn);

		// Act
		ActionResult<TransactionResponse> result = await _controller.GetTransactionById(mediatorReturn.Id);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		TransactionResponse actualControllerReturn = Assert.IsType<TransactionResponse>(okResult.Value);
		actualControllerReturn.Should().BeEquivalentTo(expectedControllerReturn);
	}

	[Fact]
	public async Task GetTransactionById_ReturnsNotFound_WhenTransactionDoesNotExist()
	{
		// Arrange
		Guid missingTransactionId = Guid.NewGuid();

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionByIdQuery>(q => q.Id == missingTransactionId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync((Transaction?)null);

		// Act
		ActionResult<TransactionResponse> result = await _controller.GetTransactionById(missingTransactionId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task GetTransactionById_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		Guid id = TransactionGenerator.Generate().Id;

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionByIdQuery>(q => q.Id == id),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<TransactionResponse> result = await _controller.GetTransactionById(id);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task GetAllTransactions_ReturnsOkResult_WithListOfTransactions()
	{
		// Arrange
		List<Transaction> mediatorReturn = TransactionGenerator.GenerateList(2);
		List<TransactionResponse> expectedControllerReturn = mediatorReturn.Select(_mapper.ToResponse).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<GetAllTransactionsQuery>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(mediatorReturn);

		// Act
		ActionResult<List<TransactionResponse>> result = await _controller.GetAllTransactions();

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<TransactionResponse> actualControllerReturn = Assert.IsType<List<TransactionResponse>>(okResult.Value);

		actualControllerReturn.Should().BeEquivalentTo(expectedControllerReturn);
	}

	[Fact]
	public async Task GetAllTransactions_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		_mediatorMock.Setup(m => m.Send(
			It.IsAny<GetAllTransactionsQuery>(),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<List<TransactionResponse>> result = await _controller.GetAllTransactions();

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task GetTransactionsByReceiptId_ReturnsOkResult_WithTransactions()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<Transaction> mediatorReturn = TransactionGenerator.GenerateList(2);
		List<TransactionResponse> expectedControllerReturn = mediatorReturn.Select(_mapper.ToResponse).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionsByReceiptIdQuery>(q => q.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(mediatorReturn);

		// Act
		ActionResult<List<TransactionResponse>?> result = await _controller.GetTransactionsByReceiptId(receiptId);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<TransactionResponse> actualControllerReturn = Assert.IsType<List<TransactionResponse>>(okResult.Value);

		actualControllerReturn.Should().BeEquivalentTo(expectedControllerReturn);
	}

	[Fact]
	public async Task GetTransactionsByReceiptId_ReturnsOkResult_WithEmptyList_WhenReceiptFoundButNoItems()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<Transaction> mediatorReturn = [];
		List<TransactionResponse> expectedControllerReturn = [];

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionsByReceiptIdQuery>(q => q.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(mediatorReturn);

		// Act
		ActionResult<List<TransactionResponse>?> result = await _controller.GetTransactionsByReceiptId(receiptId);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<TransactionResponse> actualControllerReturn = Assert.IsType<List<TransactionResponse>>(okResult.Value);

		actualControllerReturn.Should().BeEquivalentTo(expectedControllerReturn);
	}

	[Fact]
	public async Task GetTransactionsByReceiptId_ReturnsNotFound_WhenReceiptIdNotFound()
	{
		// Arrange
		Guid missingReceiptId = Guid.NewGuid();

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionsByReceiptIdQuery>(q => q.ReceiptId == missingReceiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync((List<Transaction>?)null);

		// Act
		ActionResult<List<TransactionResponse>?> result = await _controller.GetTransactionsByReceiptId(missingReceiptId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task GetTransactionsByReceiptId_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionsByReceiptIdQuery>(q => q.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<List<TransactionResponse>?> result = await _controller.GetTransactionsByReceiptId(receiptId);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task CreateTransaction_ReturnsOkResult_WithCreatedTransaction()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		Transaction transaction = TransactionGenerator.Generate();
		TransactionResponse expectedReturn = _mapper.ToResponse(transaction);

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateTransactionCommand>(c => c.Transactions.Count == 1 && c.ReceiptId == receiptId && c.AccountId == accountId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync([transaction]);

		CreateTransactionRequest controllerInput = TransactionDtoGenerator.GenerateCreateRequest();

		// Act
		ActionResult<TransactionResponse> result = await _controller.CreateTransaction(controllerInput, receiptId, accountId);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		TransactionResponse actualReturn = Assert.IsType<TransactionResponse>(okResult.Value);

		actualReturn.Should().BeEquivalentTo(expectedReturn);
	}

	[Fact]
	public async Task CreateTransaction_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		CreateTransactionRequest controllerInput = TransactionDtoGenerator.GenerateCreateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateTransactionCommand>(c => c.Transactions.Count == 1 && c.ReceiptId == receiptId && c.AccountId == accountId),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<TransactionResponse> result = await _controller.CreateTransaction(controllerInput, receiptId, accountId);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task CreateTransactions_ReturnsOkResult_WithCreatedTransactions()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		List<CreateTransactionRequest> controllerInput = TransactionDtoGenerator.GenerateCreateRequestList(2);
		List<Transaction> mediatorReturn = TransactionGenerator.GenerateList(2);
		List<TransactionResponse> expectedControllerReturn = mediatorReturn.Select(_mapper.ToResponse).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateTransactionCommand>(c => c.Transactions.Count == controllerInput.Count && c.ReceiptId == receiptId && c.AccountId == accountId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(mediatorReturn);

		// Act
		ActionResult<List<TransactionResponse>> result = await _controller.CreateTransactions(controllerInput, receiptId, accountId);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<TransactionResponse> actualControllerReturn = Assert.IsType<List<TransactionResponse>>(okResult.Value);

		actualControllerReturn.Should().BeEquivalentTo(expectedControllerReturn);
	}

	[Fact]
	public async Task CreateTransactions_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		List<CreateTransactionRequest> controllerInput = TransactionDtoGenerator.GenerateCreateRequestList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateTransactionCommand>(c => c.Transactions.Count == controllerInput.Count && c.ReceiptId == receiptId && c.AccountId == accountId),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<List<TransactionResponse>> result = await _controller.CreateTransactions(controllerInput, receiptId, accountId);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task UpdateTransaction_ReturnsNoContent_WhenUpdateSucceeds()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		UpdateTransactionRequest controllerInput = TransactionDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateTransactionCommand>(c => c.Transactions.Count == 1 && c.ReceiptId == receiptId && c.AccountId == accountId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		ActionResult<bool> result = await _controller.UpdateTransaction(controllerInput, receiptId, accountId);

		// Assert
		Assert.IsType<NoContentResult>(result.Result);
	}

	[Fact]
	public async Task UpdateTransaction_ReturnsNotFound_WhenUpdateFails()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		UpdateTransactionRequest controllerInput = TransactionDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateTransactionCommand>(c => c.Transactions.Count == 1 && c.ReceiptId == receiptId && c.AccountId == accountId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.UpdateTransaction(controllerInput, receiptId, accountId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task UpdateTransaction_ReturnsInternalServerError_WhenExceptionThrown()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		UpdateTransactionRequest controllerInput = TransactionDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateTransactionCommand>(c => c.Transactions.Count == 1 && c.ReceiptId == receiptId && c.AccountId == accountId),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<bool> result = await _controller.UpdateTransaction(controllerInput, receiptId, accountId);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task UpdateTransactions_ReturnsNoContent_WhenUpdateSucceeds()
	{
		// Arrange
		Guid accountId = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		List<UpdateTransactionRequest> controllerInput = TransactionDtoGenerator.GenerateUpdateRequestList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateTransactionCommand>(c => c.Transactions.Count == controllerInput.Count && c.ReceiptId == receiptId && c.AccountId == accountId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		ActionResult<bool> result = await _controller.UpdateTransactions(controllerInput, receiptId, accountId);

		// Assert
		Assert.IsType<NoContentResult>(result.Result);
	}

	[Fact]
	public async Task UpdateTransactions_ReturnsNotFound_WhenUpdateFails()
	{
		// Arrange
		Guid accountId = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		List<UpdateTransactionRequest> controllerInput = TransactionDtoGenerator.GenerateUpdateRequestList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateTransactionCommand>(c => c.Transactions.Count == controllerInput.Count && c.ReceiptId == receiptId && c.AccountId == accountId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.UpdateTransactions(controllerInput, receiptId, accountId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task UpdateTransactions_ReturnsInternalServerError_WhenExceptionThrown()
	{
		// Arrange
		Guid accountId = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		List<UpdateTransactionRequest> controllerInput = TransactionDtoGenerator.GenerateUpdateRequestList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateTransactionCommand>(c => c.Transactions.Count == controllerInput.Count && c.ReceiptId == receiptId && c.AccountId == accountId),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<bool> result = await _controller.UpdateTransactions(controllerInput, receiptId, accountId);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task DeleteTransactions_ReturnsNoContent_WhenDeleteSucceeds()
	{
		// Arrange
		List<Guid> controllerInput = TransactionGenerator.GenerateList(2).Select(a => a.Id).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteTransactionCommand>(c => c.Ids.SequenceEqual(controllerInput)),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		ActionResult<bool> result = await _controller.DeleteTransactions(controllerInput);

		// Assert
		Assert.IsType<NoContentResult>(result.Result);
	}

	[Fact]
	public async Task DeleteTransactions_ReturnsNotFound_WhenDeleteFails()
	{
		// Arrange
		List<Guid> controllerInput = TransactionGenerator.GenerateList(2).Select(t => t.Id).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteTransactionCommand>(c => c.Ids.SequenceEqual(controllerInput)),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.DeleteTransactions(controllerInput);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task DeleteTransactions_ReturnsNotFound_WhenMultipleTransactionsDeleteFails()
	{
		// Arrange
		List<Guid> controllerInput = TransactionGenerator.GenerateList(2).Select(t => t.Id).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteTransactionCommand>(c => c.Ids.SequenceEqual(controllerInput)),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.DeleteTransactions(controllerInput);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task DeleteTransactions_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		List<Guid> controllerInput = TransactionGenerator.GenerateList(2).Select(t => t.Id).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteTransactionCommand>(c => c.Ids.SequenceEqual(controllerInput)),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<bool> result = await _controller.DeleteTransactions(controllerInput);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}
}
