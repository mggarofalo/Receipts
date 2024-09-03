using API.Controllers;
using API.Mapping.Core;
using Application.Commands.Receipt;
using Application.Queries.Core.Receipt;
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

public class ReceiptsControllerTests
{
	private readonly IMapper _mapper;
	private readonly Mock<IMediator> _mediatorMock;
	private readonly Mock<IMapper> _mapperMock;
	private readonly Mock<ILogger<ReceiptsController>> _loggerMock;
	private readonly ReceiptsController _controller;

	public ReceiptsControllerTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<ReceiptMappingProfile>();
		});

		_mapper = configuration.CreateMapper();
		_mediatorMock = new Mock<IMediator>();
		_mapperMock = ControllerTestHelpers.GetMapperMock<Receipt, ReceiptVM>(_mapper);
		_loggerMock = ControllerTestHelpers.GetLoggerMock<ReceiptsController>();
		_controller = new ReceiptsController(_mediatorMock.Object, _mapperMock.Object, _loggerMock.Object);
	}

	[Fact]
	public async Task GetReceiptById_ReturnsOkResult_WhenReceiptExists()
	{
		// Arrange
		Receipt Receipt = ReceiptGenerator.Generate();
		ReceiptVM expectedReturn = _mapper.Map<ReceiptVM>(Receipt);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptByIdQuery>(q => q.Id == Receipt.Id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(Receipt);

		// Act
		ActionResult<ReceiptVM> result = await _controller.GetReceiptById(Receipt.Id!.Value);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		ReceiptVM actualReturn = Assert.IsType<ReceiptVM>(okResult.Value);
		Assert.Equal(expectedReturn, actualReturn);

		_mediatorMock.Verify(m => m.Send(
			It.Is<GetReceiptByIdQuery>(q => q.Id == Receipt.Id),
			It.IsAny<CancellationToken>()),
			Times.Once);

		_mapperMock.Verify(m => m.Map<Receipt, ReceiptVM>(Receipt), Times.Once);

		_loggerMock.VerifyNoErrorLoggingCalls();
		_loggerMock.VerifyNoCriticalLoggingCalls();
	}

	[Fact]
	public async Task GetReceiptById_ReturnsNotFound_WhenReceiptDoesNotExist()
	{
		// Arrange
		Guid missingReceiptId = Guid.NewGuid();

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptByIdQuery>(q => q.Id == missingReceiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync((Receipt?)null);

		// Act
		ActionResult<ReceiptVM> result = await _controller.GetReceiptById(missingReceiptId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);

		_mediatorMock.Verify(m => m.Send(
			It.Is<GetReceiptByIdQuery>(q => q.Id == missingReceiptId),
			It.IsAny<CancellationToken>()),
			Times.Once);

		_mapperMock.Verify(m => m.Map<Receipt, ReceiptVM>(It.IsAny<Receipt>()), Times.Never);

		_loggerMock.VerifyNoErrorLoggingCalls();
		_loggerMock.VerifyNoCriticalLoggingCalls();
	}

	[Fact]
	public async Task GetReceiptById_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		Guid id = ReceiptGenerator.Generate().Id!.Value;

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptByIdQuery>(q => q.Id == id),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<ReceiptVM> result = await _controller.GetReceiptById(id);

		// Assert
		Assert.IsType<ObjectResult>(result.Result);
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);

		_mediatorMock.Verify(m => m.Send(
			It.Is<GetReceiptByIdQuery>(q => q.Id == id),
			It.IsAny<CancellationToken>()),
			Times.Once);

		_mapperMock.Verify(m => m.Map<Receipt, ReceiptVM>(It.IsAny<Receipt>()), Times.Never);

		_loggerMock.VerifyErrorLoggingCalls(nameof(ReceiptsController.GetReceiptById));
		_loggerMock.VerifyNoCriticalLoggingCalls();
	}

	[Fact]
	public async Task GetAllReceipts_ReturnsOkResult_WithListOfReceipts()
	{
		// Arrange
		List<Receipt> Receipts = ReceiptGenerator.GenerateList(2);
		List<ReceiptVM> expectedReturn = _mapper.Map<List<ReceiptVM>>(Receipts);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAllReceiptsQuery>(q => true),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(Receipts);

		// Act
		ActionResult<List<ReceiptVM>> result = await _controller.GetAllReceipts();

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<ReceiptVM> actualReturn = Assert.IsType<List<ReceiptVM>>(okResult.Value);

		Assert.Equal(expectedReturn, actualReturn);

		_mediatorMock.Verify(m => m.Send(
			It.IsAny<GetAllReceiptsQuery>(),
			It.IsAny<CancellationToken>()),
			Times.Once);

		_mapperMock.Verify(m => m.Map<Receipt, ReceiptVM>(It.IsAny<Receipt>()), Times.Exactly(Receipts.Count));

		_loggerMock.VerifyNoErrorLoggingCalls();
		_loggerMock.VerifyNoCriticalLoggingCalls();
	}

	[Fact]
	public async Task GetAllReceipts_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAllReceiptsQuery>(q => true),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<List<ReceiptVM>> result = await _controller.GetAllReceipts();

		// Assert
		Assert.IsType<ObjectResult>(result.Result);
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);

		_mediatorMock.Verify(m => m.Send(
			It.IsAny<GetAllReceiptsQuery>(),
			It.IsAny<CancellationToken>()),
			Times.Once);

		_mapperMock.Verify(m => m.Map<Receipt, ReceiptVM>(It.IsAny<Receipt>()), Times.Never);

		_loggerMock.VerifyErrorLoggingCalls(nameof(ReceiptsController.GetAllReceipts));
		_loggerMock.VerifyNoCriticalLoggingCalls();
	}

	[Fact]
	public async Task CreateReceipt_ReturnsOkResult_WithCreatedReceipts()
	{
		// Arrange
		List<Receipt> Receipts = ReceiptGenerator.GenerateList(2);
		List<ReceiptVM> expectedReturn = _mapper.Map<List<ReceiptVM>>(Receipts);

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateReceiptCommand>(c => c.Receipts.Count == Receipts.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(Receipts);

		List<ReceiptVM> models = _mapper.Map<List<ReceiptVM>>(Receipts);
		models.ForEach(a => a.Id = null);

		// Act
		ActionResult<ReceiptVM> result = await _controller.CreateReceipt(models);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<ReceiptVM> actualReturn = Assert.IsType<List<ReceiptVM>>(okResult.Value);

		Assert.Equal(expectedReturn, actualReturn);

		_mediatorMock.Verify(m => m.Send(
			It.Is<CreateReceiptCommand>(c => c.Receipts.Count == models.Count),
			It.IsAny<CancellationToken>()),
			Times.Once);

		_mapperMock.Verify(m => m.Map<ReceiptVM, Receipt>(It.IsAny<ReceiptVM>()), Times.Exactly(models.Count));
		_mapperMock.Verify(m => m.Map<Receipt, ReceiptVM>(It.IsAny<Receipt>()), Times.Exactly(Receipts.Count));

		_loggerMock.VerifyNoErrorLoggingCalls();
		_loggerMock.VerifyNoCriticalLoggingCalls();
	}

	[Fact]
	public async Task CreateReceipt_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		List<ReceiptVM> models = ReceiptVMGenerator.GenerateList(2);
		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateReceiptCommand>(c => c.Receipts.Count == models.Count),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<ReceiptVM> result = await _controller.CreateReceipt(models);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);

		_mediatorMock.Verify(m => m.Send(
			It.Is<CreateReceiptCommand>(c => c.Receipts.Count == models.Count),
			It.IsAny<CancellationToken>()),
			Times.Once);

		_mapperMock.Verify(m => m.Map<ReceiptVM, Receipt>(It.IsAny<ReceiptVM>()), Times.Exactly(models.Count));
		_mapperMock.Verify(m => m.Map<Receipt, ReceiptVM>(It.IsAny<Receipt>()), Times.Never);

		_loggerMock.VerifyErrorLoggingCalls(nameof(ReceiptsController.CreateReceipt));
		_loggerMock.VerifyNoCriticalLoggingCalls();
	}

	[Fact]
	public async Task UpdateReceipts_ReturnsNoContent_WhenUpdateSucceeds()
	{
		// Arrange
		List<ReceiptVM> models = ReceiptVMGenerator.GenerateList(2);
		List<Receipt> Receipts = _mapper.Map<List<Receipt>>(models);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateReceiptCommand>(c => c.Receipts.Count == models.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		ActionResult<bool> result = await _controller.UpdateReceipts(models);

		// Assert
		NoContentResult noContentResult = Assert.IsType<NoContentResult>(result.Result);
		Assert.Equal(204, noContentResult.StatusCode);

		_mediatorMock.Verify(m => m.Send(
			It.Is<UpdateReceiptCommand>(c => c.Receipts.Count == models.Count),
			It.IsAny<CancellationToken>()),
			Times.Once);

		_mapperMock.Verify(m => m.Map<ReceiptVM, Receipt>(It.IsAny<ReceiptVM>()), Times.Exactly(models.Count));

		_loggerMock.VerifyNoErrorLoggingCalls();
		_loggerMock.VerifyNoCriticalLoggingCalls();
	}

	[Fact]
	public async Task UpdateReceipts_ReturnsNotFound_WhenUpdateFails()
	{
		// Arrange
		List<ReceiptVM> models = ReceiptVMGenerator.GenerateList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateReceiptCommand>(c => c.Receipts.Count == models.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.UpdateReceipts(models);

		// Assert
		NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
		Assert.Equal(404, notFoundResult.StatusCode);

		_mediatorMock.Verify(m => m.Send(
			It.Is<UpdateReceiptCommand>(c => c.Receipts.Count == models.Count),
			It.IsAny<CancellationToken>()),
			Times.Once);

		_mapperMock.Verify(m => m.Map<ReceiptVM, Receipt>(It.IsAny<ReceiptVM>()), Times.Exactly(models.Count));

		_loggerMock.VerifyNoErrorLoggingCalls();
		_loggerMock.VerifyNoCriticalLoggingCalls();
	}

	[Fact]
	public async Task UpdateReceipts_ReturnsInternalServerError_WhenExceptionThrown()
	{
		// Arrange
		List<ReceiptVM> models = ReceiptVMGenerator.GenerateList(2);
		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateReceiptCommand>(c => c.Receipts.Count == models.Count),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<bool> result = await _controller.UpdateReceipts(models);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);

		_mediatorMock.Verify(m => m.Send(
			It.Is<UpdateReceiptCommand>(c => c.Receipts.Count == models.Count),
			It.IsAny<CancellationToken>()),
			Times.Once);

		_mapperMock.Verify(m => m.Map<ReceiptVM, Receipt>(It.IsAny<ReceiptVM>()), Times.Exactly(models.Count));

		_loggerMock.VerifyErrorLoggingCalls(nameof(ReceiptsController.UpdateReceipts));
		_loggerMock.VerifyNoCriticalLoggingCalls();
	}

	[Fact]
	public async Task DeleteReceipts_ReturnsNoContent_WhenDeleteSucceeds()
	{
		// Arrange
		List<Guid> ids = ReceiptGenerator.GenerateList(2).Select(a => a.Id!.Value).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteReceiptCommand>(c => c.Ids.SequenceEqual(ids)),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		ActionResult<bool> result = await _controller.DeleteReceipts(ids);

		// Assert
		NoContentResult noContentResult = Assert.IsType<NoContentResult>(result.Result);
		Assert.Equal(204, noContentResult.StatusCode);

		_mediatorMock.Verify(m => m.Send(
			It.Is<DeleteReceiptCommand>(c => c.Ids.SequenceEqual(ids)),
			It.IsAny<CancellationToken>()),
			Times.Once);

		_loggerMock.VerifyNoErrorLoggingCalls();
		_loggerMock.VerifyNoCriticalLoggingCalls();
	}

	[Fact]
	public async Task DeleteReceipts_ReturnsNotFound_WhenSingleReceiptDeleteFails()
	{
		// Arrange
		List<Guid> ids = [ReceiptGenerator.Generate().Id!.Value];

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<DeleteReceiptCommand>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.DeleteReceipts(ids);

		// Assert
		NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
		Assert.Equal(404, notFoundResult.StatusCode);

		_mediatorMock.Verify(m => m.Send(
			It.Is<DeleteReceiptCommand>(c => c.Ids.SequenceEqual(ids)),
			It.IsAny<CancellationToken>()),
			Times.Once);

		_loggerMock.VerifyNoErrorLoggingCalls();
		_loggerMock.VerifyNoCriticalLoggingCalls();
	}

	[Fact]
	public async Task DeleteReceipts_ReturnsNotFound_WhenMultipleReceiptsDeleteFails()
	{
		// Arrange
		List<Guid> ids = ReceiptGenerator.GenerateList(2).Select(a => a.Id!.Value).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<DeleteReceiptCommand>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.DeleteReceipts(ids);

		// Assert
		NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
		Assert.Equal(404, notFoundResult.StatusCode);

		_mediatorMock.Verify(m => m.Send(
			It.Is<DeleteReceiptCommand>(c => c.Ids.SequenceEqual(ids)),
			It.IsAny<CancellationToken>()),
			Times.Once);

		_loggerMock.VerifyNoErrorLoggingCalls();
		_loggerMock.VerifyNoCriticalLoggingCalls();
	}

	[Fact]
	public async Task DeleteReceipts_ReturnsInternalServerError_WhenExceptionThrown()
	{
		// Arrange
		List<Guid> ids = ReceiptGenerator.GenerateList(2).Select(a => a.Id!.Value).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<DeleteReceiptCommand>(),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<bool> result = await _controller.DeleteReceipts(ids);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);

		_mediatorMock.Verify(m => m.Send(
			It.Is<DeleteReceiptCommand>(c => c.Ids.SequenceEqual(ids)),
			It.IsAny<CancellationToken>()),
			Times.Once);

		_loggerMock.VerifyErrorLoggingCalls(nameof(ReceiptsController.DeleteReceipts));
		_loggerMock.VerifyNoCriticalLoggingCalls();
	}
}