using API.Controllers.Core;
using API.Mapping.Core;
using Application.Commands.Receipt.Create;
using Application.Commands.Receipt.Update;
using Application.Commands.Receipt.Delete;
using Application.Queries.Core.Receipt;
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

public class ReceiptsControllerTests
{
	private readonly ReceiptMapper _mapper;
	private readonly Mock<IMediator> _mediatorMock;
	private readonly Mock<ILogger<ReceiptsController>> _loggerMock;
	private readonly ReceiptsController _controller;

	public ReceiptsControllerTests()
	{
		_mediatorMock = new Mock<IMediator>();
		_mapper = new ReceiptMapper();
		_loggerMock = ControllerTestHelpers.GetLoggerMock<ReceiptsController>();
		_controller = new ReceiptsController(_mediatorMock.Object, _mapper, _loggerMock.Object);
	}

	[Fact]
	public async Task GetReceiptById_ReturnsOkResult_WithReceipt()
	{
		// Arrange
		Receipt mediatorReturn = ReceiptGenerator.Generate();
		ReceiptVM expectedControllerReturn = _mapper.ToViewModel(mediatorReturn);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptByIdQuery>(q => q.Id == mediatorReturn.Id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(mediatorReturn);

		// Act
		ActionResult<ReceiptVM> result = await _controller.GetReceiptById(mediatorReturn.Id!.Value);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		ReceiptVM actualControllerReturn = Assert.IsType<ReceiptVM>(okResult.Value);
		actualControllerReturn.Should().BeEquivalentTo(expectedControllerReturn);
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
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task GetAllReceipts_ReturnsOkResult_WithListOfReceipts()
	{
		// Arrange
		List<Receipt> mediatorReturn = ReceiptGenerator.GenerateList(2);
		List<ReceiptVM> expectedControllerReturn = mediatorReturn.Select(_mapper.ToViewModel).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<GetAllReceiptsQuery>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(mediatorReturn);

		// Act
		ActionResult<List<ReceiptVM>> result = await _controller.GetAllReceipts();

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<ReceiptVM> actualControllerReturn = Assert.IsType<List<ReceiptVM>>(okResult.Value);

		actualControllerReturn.Should().BeEquivalentTo(expectedControllerReturn);
	}

	[Fact]
	public async Task GetAllReceipts_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		_mediatorMock.Setup(m => m.Send(
			It.IsAny<GetAllReceiptsQuery>(),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<List<ReceiptVM>> result = await _controller.GetAllReceipts();

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task CreateReceipts_ReturnsOkResult_WithCreatedReceipts()
	{
		// Arrange
		List<Receipt> mediatorReturn = ReceiptGenerator.GenerateList(2);
		List<ReceiptVM> expectedControllerReturn = mediatorReturn.Select(_mapper.ToViewModel).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateReceiptCommand>(c => c.Receipts.Count == mediatorReturn.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(mediatorReturn);

		List<ReceiptVM> controllerInput = mediatorReturn.Select(_mapper.ToViewModel).ToList().WithNullIds();

		// Act
		ActionResult<List<ReceiptVM>> result = await _controller.CreateReceipts(controllerInput);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<ReceiptVM> actualControllerReturn = Assert.IsType<List<ReceiptVM>>(okResult.Value);

		actualControllerReturn.Should().BeEquivalentTo(expectedControllerReturn);
	}

	[Fact]
	public async Task CreateReceipts_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		List<ReceiptVM> controllerInput = ReceiptVMGenerator.GenerateList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateReceiptCommand>(c => c.Receipts.Count == controllerInput.Count),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<List<ReceiptVM>> result = await _controller.CreateReceipts(controllerInput);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task UpdateReceipts_ReturnsNoContent_WhenUpdateSucceeds()
	{
		// Arrange
		List<ReceiptVM> controllerInput = ReceiptVMGenerator.GenerateList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateReceiptCommand>(c => c.Receipts.Count == controllerInput.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		ActionResult<bool> result = await _controller.UpdateReceipts(controllerInput);

		// Assert
		Assert.IsType<NoContentResult>(result.Result);
	}

	[Fact]
	public async Task UpdateReceipts_ReturnsNotFound_WhenUpdateFails()
	{
		// Arrange
		List<ReceiptVM> controllerInput = ReceiptVMGenerator.GenerateList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateReceiptCommand>(c => c.Receipts.Count == controllerInput.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.UpdateReceipts(controllerInput);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task UpdateReceipts_ReturnsInternalServerError_WhenExceptionThrown()
	{
		// Arrange
		List<ReceiptVM> controllerInput = ReceiptVMGenerator.GenerateList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateReceiptCommand>(c => c.Receipts.Count == controllerInput.Count),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<bool> result = await _controller.UpdateReceipts(controllerInput);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task DeleteReceipts_ReturnsNoContent_WhenDeleteSucceeds()
	{
		// Arrange
		List<Guid> controllerInput = ReceiptGenerator.GenerateList(2).Select(a => a.Id!.Value).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteReceiptCommand>(c => c.Ids.SequenceEqual(controllerInput)),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		ActionResult<bool> result = await _controller.DeleteReceipts(controllerInput);

		// Assert
		Assert.IsType<NoContentResult>(result.Result);
	}

	[Fact]
	public async Task DeleteReceipts_ReturnsNotFound_WhenDeleteFails()
	{
		// Arrange
		List<Guid> controllerInput = ReceiptGenerator.GenerateList(2).Select(a => a.Id!.Value).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteReceiptCommand>(c => c.Ids.SequenceEqual(controllerInput)),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.DeleteReceipts(controllerInput);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task DeleteReceipts_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		List<Guid> controllerInput = ReceiptGenerator.GenerateList(2).Select(a => a.Id!.Value).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteReceiptCommand>(c => c.Ids.SequenceEqual(controllerInput)),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<bool> result = await _controller.DeleteReceipts(controllerInput);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}
}