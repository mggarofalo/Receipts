using API.Controllers.Core;
using API.Generated.Dtos;
using API.Mapping.Core;
using Application.Commands.Receipt.Create;
using Application.Commands.Receipt.Delete;
using Application.Commands.Receipt.Update;
using Application.Queries.Core.Receipt;
using Domain.Core;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SampleData.Domain.Core;
using SampleData.Dtos.Core;

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
		ReceiptResponse expectedControllerReturn = _mapper.ToResponse(mediatorReturn);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptByIdQuery>(q => q.Id == mediatorReturn.Id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(mediatorReturn);

		// Act
		ActionResult<ReceiptResponse> result = await _controller.GetReceiptById(mediatorReturn.Id);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		ReceiptResponse actualControllerReturn = Assert.IsType<ReceiptResponse>(okResult.Value);
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
		ActionResult<ReceiptResponse> result = await _controller.GetReceiptById(missingReceiptId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task GetReceiptById_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		Guid id = ReceiptGenerator.Generate().Id;

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptByIdQuery>(q => q.Id == id),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<ReceiptResponse> result = await _controller.GetReceiptById(id);

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
		List<ReceiptResponse> expectedControllerReturn = [.. mediatorReturn.Select(_mapper.ToResponse)];

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<GetAllReceiptsQuery>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(mediatorReturn);

		// Act
		ActionResult<List<ReceiptResponse>> result = await _controller.GetAllReceipts();

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<ReceiptResponse> actualControllerReturn = Assert.IsType<List<ReceiptResponse>>(okResult.Value);

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
		ActionResult<List<ReceiptResponse>> result = await _controller.GetAllReceipts();

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task CreateReceipt_ReturnsOkResult_WithCreatedReceipt()
	{
		// Arrange
		Receipt receipt = ReceiptGenerator.Generate();
		ReceiptResponse expectedReturn = _mapper.ToResponse(receipt);

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateReceiptCommand>(c => c.Receipts.Count == 1),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync([receipt]);

		CreateReceiptRequest controllerInput = ReceiptDtoGenerator.GenerateCreateRequest();

		// Act
		ActionResult<ReceiptResponse> result = await _controller.CreateReceipt(controllerInput);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		ReceiptResponse actualReturn = Assert.IsType<ReceiptResponse>(okResult.Value);

		actualReturn.Should().BeEquivalentTo(expectedReturn);
	}

	[Fact]
	public async Task CreateReceipt_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		CreateReceiptRequest controllerInput = ReceiptDtoGenerator.GenerateCreateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateReceiptCommand>(c => c.Receipts.Count == 1),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<ReceiptResponse> result = await _controller.CreateReceipt(controllerInput);

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
		List<ReceiptResponse> expectedControllerReturn = [.. mediatorReturn.Select(_mapper.ToResponse)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateReceiptCommand>(c => c.Receipts.Count == mediatorReturn.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(mediatorReturn);

		List<CreateReceiptRequest> controllerInput = ReceiptDtoGenerator.GenerateCreateRequestList(2);

		// Act
		ActionResult<List<ReceiptResponse>> result = await _controller.CreateReceipts(controllerInput);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<ReceiptResponse> actualControllerReturn = Assert.IsType<List<ReceiptResponse>>(okResult.Value);

		actualControllerReturn.Should().BeEquivalentTo(expectedControllerReturn);
	}

	[Fact]
	public async Task CreateReceipts_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		List<CreateReceiptRequest> controllerInput = ReceiptDtoGenerator.GenerateCreateRequestList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateReceiptCommand>(c => c.Receipts.Count == controllerInput.Count),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<List<ReceiptResponse>> result = await _controller.CreateReceipts(controllerInput);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task UpdateReceipt_ReturnsNoContent_WhenUpdateSucceeds()
	{
		// Arrange
		UpdateReceiptRequest controllerInput = ReceiptDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateReceiptCommand>(c => c.Receipts.Count == 1),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		ActionResult<bool> result = await _controller.UpdateReceipt(controllerInput.Id, controllerInput);

		// Assert
		Assert.IsType<NoContentResult>(result.Result);
	}

	[Fact]
	public async Task UpdateReceipt_ReturnsNotFound_WhenUpdateFails()
	{
		// Arrange
		UpdateReceiptRequest controllerInput = ReceiptDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateReceiptCommand>(c => c.Receipts.Count == 1),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.UpdateReceipt(controllerInput.Id, controllerInput);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task UpdateReceipt_ReturnsInternalServerError_WhenExceptionThrown()
	{
		// Arrange
		UpdateReceiptRequest controllerInput = ReceiptDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateReceiptCommand>(c => c.Receipts.Count == 1),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<bool> result = await _controller.UpdateReceipt(controllerInput.Id, controllerInput);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task UpdateReceipts_ReturnsNoContent_WhenUpdateSucceeds()
	{
		// Arrange
		List<UpdateReceiptRequest> controllerInput = ReceiptDtoGenerator.GenerateUpdateRequestList(2);

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
		List<UpdateReceiptRequest> controllerInput = ReceiptDtoGenerator.GenerateUpdateRequestList(2);

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
		List<UpdateReceiptRequest> controllerInput = ReceiptDtoGenerator.GenerateUpdateRequestList(2);

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
		List<Guid> controllerInput = [.. ReceiptGenerator.GenerateList(2).Select(a => a.Id)];

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
		List<Guid> controllerInput = [.. ReceiptGenerator.GenerateList(2).Select(a => a.Id)];

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
		List<Guid> controllerInput = [.. ReceiptGenerator.GenerateList(2).Select(a => a.Id)];

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
