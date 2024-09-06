using API.Controllers.Core;
using Application.Commands.Transaction;
using Application.Queries.Core.Transaction;
using AutoMapper;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SampleData.Domain.Core;
using SampleData.ViewModels.Core;
using Shared.Mapping.Core;
using Shared.ViewModels.Core;

namespace Presentation.API.Tests.Controllers.Core;

public class TransactionsControllerTests
{
	private readonly IMapper _mapper;
	private readonly Mock<IMediator> _mediatorMock;
	private readonly Mock<IMapper> _mapperMock;
	private readonly Mock<ILogger<TransactionsController>> _loggerMock;
	private readonly TransactionsController _controller;

	public TransactionsControllerTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<TransactionMappingProfile>();
		});

		_mapper = configuration.CreateMapper();
		_mediatorMock = new Mock<IMediator>();
		_mapperMock = ControllerTestHelpers.GetMapperMock<Transaction, TransactionVM>(_mapper);
		_loggerMock = ControllerTestHelpers.GetLoggerMock<TransactionsController>();
		_controller = new TransactionsController(_mediatorMock.Object, _mapperMock.Object, _loggerMock.Object);
	}

	[Fact]
	public async Task GetTransactionById_ReturnsOkResult_WhenTransactionExists()
	{
		// Arrange
		Transaction Transaction = TransactionGenerator.Generate();
		TransactionVM expectedReturn = _mapper.Map<TransactionVM>(Transaction);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionByIdQuery>(q => q.Id == Transaction.Id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(Transaction);

		// Act
		ActionResult<TransactionVM> result = await _controller.GetTransactionById(Transaction.Id!.Value);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		TransactionVM actualReturn = Assert.IsType<TransactionVM>(okResult.Value);
		Assert.Equal(expectedReturn, actualReturn);
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
		Assert.IsType<ObjectResult>(result.Result);
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task GetAllTransactions_ReturnsOkResult_WithListOfTransactions()
	{
		// Arrange
		List<Transaction> Transactions = TransactionGenerator.GenerateList(2);
		List<TransactionVM> expectedReturn = _mapper.Map<List<TransactionVM>>(Transactions);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAllTransactionsQuery>(q => true),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(Transactions);

		// Act
		ActionResult<List<TransactionVM>> result = await _controller.GetAllTransactions();

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<TransactionVM> actualReturn = Assert.IsType<List<TransactionVM>>(okResult.Value);

		Assert.Equal(expectedReturn, actualReturn);
	}

	[Fact]
	public async Task GetAllAccounts_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAllTransactionsQuery>(q => true),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<List<TransactionVM>> result = await _controller.GetAllTransactions();

		// Assert
		Assert.IsType<ObjectResult>(result.Result);
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task GetTransactionsByReceiptId_ReturnsOkResult_WithTransactions()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<Transaction> Transactions = TransactionGenerator.GenerateList(2);
		List<TransactionVM> expectedReturn = _mapper.Map<List<TransactionVM>>(Transactions);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionsByReceiptIdQuery>(q => q.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(Transactions);

		// Act
		ActionResult<List<TransactionVM>?> result = await _controller.GetTransactionsByReceiptId(receiptId);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<TransactionVM> actualReturn = Assert.IsType<List<TransactionVM>>(okResult.Value);

		Assert.Equal(expectedReturn, actualReturn);
	}

	[Fact]
	public async Task GetTransactionsByReceiptId_ReturnsOkResult_WithEmptyList_WhenReceiptFoundButNoItems()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<Transaction> Transactions = [];
		List<TransactionVM> expectedReturn = _mapper.Map<List<TransactionVM>>(Transactions);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTransactionsByReceiptIdQuery>(q => q.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(Transactions);

		// Act
		ActionResult<List<TransactionVM>?> result = await _controller.GetTransactionsByReceiptId(receiptId);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<TransactionVM> actualReturn = Assert.IsType<List<TransactionVM>>(okResult.Value);

		Assert.Equal(expectedReturn, actualReturn);
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
		NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
		Assert.Equal(404, notFoundResult.StatusCode);
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
		Assert.IsType<ObjectResult>(result.Result);
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task CreateTransactions_ReturnsOkResult_WithCreatedTransactions()
	{
		// Arrange
		List<Transaction> Transactions = TransactionGenerator.GenerateList(2);
		List<TransactionVM> expectedReturn = _mapper.Map<List<TransactionVM>>(Transactions);

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateTransactionCommand>(c => c.Transactions.Count == Transactions.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(Transactions);

		List<TransactionVM> models = _mapper.Map<List<TransactionVM>>(Transactions);
		models.ForEach(a => a.Id = null);

		// Act
		ActionResult<TransactionVM> result = await _controller.CreateTransactions(models);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<TransactionVM> actualReturn = Assert.IsType<List<TransactionVM>>(okResult.Value);

		Assert.Equal(expectedReturn, actualReturn);
	}

	[Fact]
	public async Task CreateTransactions_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		List<TransactionVM> models = TransactionVMGenerator.GenerateList(2);
		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateTransactionCommand>(c => c.Transactions.Count == models.Count),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<TransactionVM> result = await _controller.CreateTransactions(models);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task UpdateTransactions_ReturnsNoContent_WhenUpdateSucceeds()
	{
		// Arrange
		List<TransactionVM> models = TransactionVMGenerator.GenerateList(2, receiptId: Guid.NewGuid(), accountId: Guid.NewGuid());
		List<Transaction> Transactions = _mapper.Map<List<Transaction>>(models);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateTransactionCommand>(c => c.Transactions.Count == models.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		ActionResult<bool> result = await _controller.UpdateTransactions(models);

		// Assert
		NoContentResult noContentResult = Assert.IsType<NoContentResult>(result.Result);
		Assert.Equal(204, noContentResult.StatusCode);
	}

	[Fact]
	public async Task UpdateTransactions_ReturnsNotFound_WhenUpdateFails()
	{
		// Arrange
		List<TransactionVM> models = TransactionVMGenerator.GenerateList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateTransactionCommand>(c => c.Transactions.Count == models.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.UpdateTransactions(models);

		// Assert
		NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
		Assert.Equal(404, notFoundResult.StatusCode);
	}

	[Fact]
	public async Task UpdateTransactions_ReturnsInternalServerError_WhenExceptionThrown()
	{
		// Arrange
		List<TransactionVM> models = TransactionVMGenerator.GenerateList(2);
		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateTransactionCommand>(c => c.Transactions.Count == models.Count),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<bool> result = await _controller.UpdateTransactions(models);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task DeleteTransactions_ReturnsNoContent_WhenDeleteSucceeds()
	{
		// Arrange
		List<Guid> ids = TransactionGenerator.GenerateList(2).Select(a => a.Id!.Value).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteTransactionCommand>(c => c.Ids.SequenceEqual(ids)),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		ActionResult<bool> result = await _controller.DeleteTransactions(ids);

		// Assert
		NoContentResult noContentResult = Assert.IsType<NoContentResult>(result.Result);
		Assert.Equal(204, noContentResult.StatusCode);
	}

	[Fact]
	public async Task DeleteTransactions_ReturnsNotFound_WhenSingleTransactionDeleteFails()
	{
		// Arrange
		List<Guid> ids = [AccountGenerator.Generate().Id!.Value];

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<DeleteTransactionCommand>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.DeleteTransactions(ids);

		// Assert
		NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
		Assert.Equal(404, notFoundResult.StatusCode);
	}

	[Fact]
	public async Task DeleteTransactions_ReturnsNotFound_WhenMultipleTransactionsDeleteFails()
	{
		// Arrange
		List<Guid> ids = AccountGenerator.GenerateList(2).Select(a => a.Id!.Value).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<DeleteTransactionCommand>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.DeleteTransactions(ids);

		// Assert
		NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
		Assert.Equal(404, notFoundResult.StatusCode);
	}

	[Fact]
	public async Task DeleteTransactions_ReturnsInternalServerError_WhenExceptionThrown()
	{
		// Arrange
		List<Guid> ids = AccountGenerator.GenerateList(2).Select(a => a.Id!.Value).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<DeleteTransactionCommand>(),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<bool> result = await _controller.DeleteTransactions(ids);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}
}