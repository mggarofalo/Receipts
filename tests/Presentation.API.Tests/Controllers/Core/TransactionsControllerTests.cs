using API.Controllers.Core;
using API.Mapping.Core;
using Application.Commands.Transaction.Create;
using Application.Commands.Transaction.Update;
using Application.Commands.Transaction.Delete;
using Application.Queries.Core.Transaction;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SampleData.Domain.Core;
using SampleData.ViewModels.Core;
using Shared.ViewModels.Core;
using FluentAssertions;

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
		TransactionVM expectedControllerReturn = _mapper.ToViewModel(mediatorReturn);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionByIdQuery>(q => q.Id == mediatorReturn.Id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(mediatorReturn);

		// Act
		ActionResult<TransactionVM> result = await _controller.GetTransactionById(mediatorReturn.Id!.Value);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		TransactionVM actualControllerReturn = Assert.IsType<TransactionVM>(okResult.Value);
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
		ActionResult<TransactionVM> result = await _controller.GetTransactionById(missingTransactionId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task GetTransactionById_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		Guid id = TransactionGenerator.Generate().Id!.Value;

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionByIdQuery>(q => q.Id == id),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<TransactionVM> result = await _controller.GetTransactionById(id);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task GetAllTransactions_ReturnsOkResult_WithListOfTransactions()
	{
		// Arrange
		List<Transaction> mediatorReturn = TransactionGenerator.GenerateList(2);
		List<TransactionVM> expectedControllerReturn = mediatorReturn.Select(_mapper.ToViewModel).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<GetAllTransactionsQuery>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(mediatorReturn);

		// Act
		ActionResult<List<TransactionVM>> result = await _controller.GetAllTransactions();

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<TransactionVM> actualControllerReturn = Assert.IsType<List<TransactionVM>>(okResult.Value);

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
		ActionResult<List<TransactionVM>> result = await _controller.GetAllTransactions();

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
		List<TransactionVM> expectedControllerReturn = mediatorReturn.Select(_mapper.ToViewModel).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionsByReceiptIdQuery>(q => q.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(mediatorReturn);

		// Act
		ActionResult<List<TransactionVM>?> result = await _controller.GetTransactionsByReceiptId(receiptId);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<TransactionVM> actualControllerReturn = Assert.IsType<List<TransactionVM>>(okResult.Value);

		actualControllerReturn.Should().BeEquivalentTo(expectedControllerReturn);
	}

	[Fact]
	public async Task GetTransactionsByReceiptId_ReturnsOkResult_WithEmptyList_WhenReceiptFoundButNoItems()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<Transaction> mediatorReturn = [];
		List<TransactionVM> expectedControllerReturn = [];

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionsByReceiptIdQuery>(q => q.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(mediatorReturn);

		// Act
		ActionResult<List<TransactionVM>?> result = await _controller.GetTransactionsByReceiptId(receiptId);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<TransactionVM> actualControllerReturn = Assert.IsType<List<TransactionVM>>(okResult.Value);

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
		ActionResult<List<TransactionVM>?> result = await _controller.GetTransactionsByReceiptId(missingReceiptId);

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
		ActionResult<List<TransactionVM>?> result = await _controller.GetTransactionsByReceiptId(receiptId);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task CreateTransactions_ReturnsOkResult_WithCreatedTransactions()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		List<TransactionVM> controllerInput = TransactionVMGenerator.GenerateList(2);
		List<Transaction> mediatorReturn = controllerInput.Select(_mapper.ToDomain).ToList();
		List<TransactionVM> expectedControllerReturn = mediatorReturn.Select(_mapper.ToViewModel).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateTransactionCommand>(c => c.Transactions.Count == controllerInput.Count && c.ReceiptId == receiptId && c.AccountId == accountId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(mediatorReturn);

		// Act
		ActionResult<List<TransactionVM>> result = await _controller.CreateTransactions(controllerInput, receiptId, accountId);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<TransactionVM> actualControllerReturn = Assert.IsType<List<TransactionVM>>(okResult.Value);

		actualControllerReturn.Should().BeEquivalentTo(expectedControllerReturn);
	}

	[Fact]
	public async Task CreateTransactions_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		List<TransactionVM> controllerInput = TransactionVMGenerator.GenerateList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateTransactionCommand>(c => c.Transactions.Count == controllerInput.Count && c.ReceiptId == receiptId && c.AccountId == accountId),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<List<TransactionVM>> result = await _controller.CreateTransactions(controllerInput, receiptId, accountId);

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
		List<TransactionVM> controllerInput = TransactionVMGenerator.GenerateList(2);

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
		List<TransactionVM> controllerInput = TransactionVMGenerator.GenerateList(2);

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
		List<TransactionVM> controllerInput = TransactionVMGenerator.GenerateList(2);

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
		List<Guid> controllerInput = TransactionGenerator.GenerateList(2).Select(a => a.Id!.Value).ToList();

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
		List<Guid> controllerInput = TransactionGenerator.GenerateList(2).Select(t => t.Id!.Value).ToList();

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
		List<Guid> controllerInput = TransactionGenerator.GenerateList(2).Select(t => t.Id!.Value).ToList();

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
		List<Guid> controllerInput = TransactionGenerator.GenerateList(2).Select(t => t.Id!.Value).ToList();

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