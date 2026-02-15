using API.Controllers.Core;
using API.Generated.Dtos;
using API.Mapping.Core;
using Application.Commands.ReceiptItem.Create;
using Application.Commands.ReceiptItem.Delete;
using Application.Commands.ReceiptItem.Update;
using Application.Queries.Core.ReceiptItem;
using Domain.Core;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SampleData.Domain.Core;
using SampleData.Dtos.Core;

namespace Presentation.API.Tests.Controllers.Core;

public class ReceiptItemsControllerTests
{
	private readonly ReceiptItemMapper _mapper;
	private readonly Mock<IMediator> _mediatorMock;
	private readonly Mock<ILogger<ReceiptItemsController>> _loggerMock;
	private readonly ReceiptItemsController _controller;

	public ReceiptItemsControllerTests()
	{
		_mediatorMock = new Mock<IMediator>();
		_mapper = new ReceiptItemMapper();
		_loggerMock = ControllerTestHelpers.GetLoggerMock<ReceiptItemsController>();
		_controller = new ReceiptItemsController(_mediatorMock.Object, _mapper, _loggerMock.Object);
	}

	[Fact]
	public async Task GetReceiptItemById_ReturnsOkResult_WithReceiptItem()
	{
		// Arrange
		ReceiptItem mediatorReturn = ReceiptItemGenerator.Generate();
		ReceiptItemResponse expectedControllerReturn = _mapper.ToResponse(mediatorReturn);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptItemByIdQuery>(q => q.Id == mediatorReturn.Id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(mediatorReturn);

		// Act
		ActionResult<ReceiptItemResponse> result = await _controller.GetReceiptItemById(mediatorReturn.Id);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		ReceiptItemResponse actualControllerReturn = Assert.IsType<ReceiptItemResponse>(okResult.Value);
		actualControllerReturn.Should().BeEquivalentTo(expectedControllerReturn);
	}

	[Fact]
	public async Task GetReceiptItemById_ReturnsNotFound_WhenReceiptItemDoesNotExist()
	{
		// Arrange
		Guid missingReceiptItemId = Guid.NewGuid();

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptItemByIdQuery>(q => q.Id == missingReceiptItemId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync((ReceiptItem?)null);

		// Act
		ActionResult<ReceiptItemResponse> result = await _controller.GetReceiptItemById(missingReceiptItemId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task GetReceiptItemById_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		Guid id = ReceiptItemGenerator.Generate().Id;

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptItemByIdQuery>(q => q.Id == id),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<ReceiptItemResponse> result = await _controller.GetReceiptItemById(id);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task GetAllReceiptItems_ReturnsOkResult_WithListOfReceiptItems()
	{
		// Arrange
		List<ReceiptItem> mediatorReturn = ReceiptItemGenerator.GenerateList(2);
		List<ReceiptItemResponse> expectedControllerReturn = [.. mediatorReturn.Select(_mapper.ToResponse)];

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<GetAllReceiptItemsQuery>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(mediatorReturn);

		// Act
		ActionResult<List<ReceiptItemResponse>> result = await _controller.GetAllReceiptItems();

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<ReceiptItemResponse> actualControllerReturn = Assert.IsType<List<ReceiptItemResponse>>(okResult.Value);

		actualControllerReturn.Should().BeEquivalentTo(expectedControllerReturn);
	}

	[Fact]
	public async Task GetAllReceiptItems_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		_mediatorMock.Setup(m => m.Send(
			It.IsAny<GetAllReceiptItemsQuery>(),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<List<ReceiptItemResponse>> result = await _controller.GetAllReceiptItems();

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task GetReceiptItemsByReceiptId_ReturnsOkResult_WithReceiptItems()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<ReceiptItem> mediatorReturn = ReceiptItemGenerator.GenerateList(2);
		List<ReceiptItemResponse> expectedControllerReturn = [.. mediatorReturn.Select(_mapper.ToResponse)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptItemsByReceiptIdQuery>(q => q.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(mediatorReturn);

		// Act
		ActionResult<List<ReceiptItemResponse>?> result = await _controller.GetReceiptItemsByReceiptId(receiptId);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<ReceiptItemResponse> actualControllerReturn = Assert.IsType<List<ReceiptItemResponse>>(okResult.Value);

		actualControllerReturn.Should().BeEquivalentTo(expectedControllerReturn);
	}

	[Fact]
	public async Task GetReceiptItemsByReceiptId_ReturnsOkResult_WithEmptyList_WhenReceiptFoundButNoItems()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<ReceiptItem> mediatorReturn = [];
		List<ReceiptItemResponse> expectedControllerReturn = [];

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptItemsByReceiptIdQuery>(q => q.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(mediatorReturn);

		// Act
		ActionResult<List<ReceiptItemResponse>?> result = await _controller.GetReceiptItemsByReceiptId(receiptId);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<ReceiptItemResponse> actualControllerReturn = Assert.IsType<List<ReceiptItemResponse>>(okResult.Value);

		actualControllerReturn.Should().BeEquivalentTo(expectedControllerReturn);
	}

	[Fact]
	public async Task GetReceiptItemsByReceiptId_ReturnsNotFound_WhenReceiptIdNotFound()
	{
		// Arrange
		Guid missingReceiptId = Guid.NewGuid();

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptItemsByReceiptIdQuery>(q => q.ReceiptId == missingReceiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync((List<ReceiptItem>?)null);

		// Act
		ActionResult<List<ReceiptItemResponse>?> result = await _controller.GetReceiptItemsByReceiptId(missingReceiptId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task GetReceiptItemsByReceiptId_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptItemsByReceiptIdQuery>(q => q.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<List<ReceiptItemResponse>?> result = await _controller.GetReceiptItemsByReceiptId(receiptId);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task CreateReceiptItem_ReturnsOkResult_WithCreatedReceiptItem()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		ReceiptItem receiptItem = ReceiptItemGenerator.Generate();
		ReceiptItemResponse expectedReturn = _mapper.ToResponse(receiptItem);

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateReceiptItemCommand>(c => c.ReceiptItems.Count == 1 && c.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync([receiptItem]);

		CreateReceiptItemRequest controllerInput = ReceiptItemDtoGenerator.GenerateCreateRequest();

		// Act
		ActionResult<ReceiptItemResponse> result = await _controller.CreateReceiptItem(controllerInput, receiptId);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		ReceiptItemResponse actualReturn = Assert.IsType<ReceiptItemResponse>(okResult.Value);

		actualReturn.Should().BeEquivalentTo(expectedReturn);
	}

	[Fact]
	public async Task CreateReceiptItem_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		CreateReceiptItemRequest controllerInput = ReceiptItemDtoGenerator.GenerateCreateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateReceiptItemCommand>(c => c.ReceiptItems.Count == 1 && c.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<ReceiptItemResponse> result = await _controller.CreateReceiptItem(controllerInput, receiptId);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task CreateReceiptItems_ReturnsOkResult_WithCreatedReceiptItems()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<ReceiptItem> mediatorReturn = ReceiptItemGenerator.GenerateList(2);
		List<ReceiptItemResponse> expectedControllerReturn = [.. mediatorReturn.Select(_mapper.ToResponse)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateReceiptItemCommand>(c => c.ReceiptItems.Count == mediatorReturn.Count && c.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(mediatorReturn);

		List<CreateReceiptItemRequest> controllerInput = ReceiptItemDtoGenerator.GenerateCreateRequestList(2);

		// Act
		ActionResult<List<ReceiptItemResponse>> result = await _controller.CreateReceiptItems(controllerInput, receiptId);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<ReceiptItemResponse> actualControllerReturn = Assert.IsType<List<ReceiptItemResponse>>(okResult.Value);

		actualControllerReturn.Should().BeEquivalentTo(expectedControllerReturn);
	}

	[Fact]
	public async Task CreateReceiptItems_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<CreateReceiptItemRequest> controllerInput = ReceiptItemDtoGenerator.GenerateCreateRequestList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateReceiptItemCommand>(c => c.ReceiptItems.Count == controllerInput.Count && c.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<List<ReceiptItemResponse>> result = await _controller.CreateReceiptItems(controllerInput, receiptId);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task UpdateReceiptItem_ReturnsNoContent_WhenUpdateSucceeds()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		UpdateReceiptItemRequest controllerInput = ReceiptItemDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateReceiptItemCommand>(c => c.ReceiptItems.Count == 1 && c.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		ActionResult<bool> result = await _controller.UpdateReceiptItem(controllerInput, receiptId);

		// Assert
		Assert.IsType<NoContentResult>(result.Result);
	}

	[Fact]
	public async Task UpdateReceiptItem_ReturnsNotFound_WhenUpdateFails()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		UpdateReceiptItemRequest controllerInput = ReceiptItemDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateReceiptItemCommand>(c => c.ReceiptItems.Count == 1 && c.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.UpdateReceiptItem(controllerInput, receiptId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task UpdateReceiptItem_ReturnsInternalServerError_WhenExceptionThrown()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		UpdateReceiptItemRequest controllerInput = ReceiptItemDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateReceiptItemCommand>(c => c.ReceiptItems.Count == 1 && c.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<bool> result = await _controller.UpdateReceiptItem(controllerInput, receiptId);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task UpdateReceiptItems_ReturnsNoContent_WhenUpdateSucceeds()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<UpdateReceiptItemRequest> controllerInput = ReceiptItemDtoGenerator.GenerateUpdateRequestList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateReceiptItemCommand>(c => c.ReceiptItems.Count == controllerInput.Count && c.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		ActionResult<bool> result = await _controller.UpdateReceiptItems(controllerInput, receiptId);

		// Assert
		Assert.IsType<NoContentResult>(result.Result);
	}

	[Fact]
	public async Task UpdateReceiptItems_ReturnsNotFound_WhenUpdateFails()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<UpdateReceiptItemRequest> controllerInput = ReceiptItemDtoGenerator.GenerateUpdateRequestList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateReceiptItemCommand>(c => c.ReceiptItems.Count == controllerInput.Count && c.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.UpdateReceiptItems(controllerInput, receiptId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task UpdateReceiptItems_ReturnsInternalServerError_WhenExceptionThrown()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<UpdateReceiptItemRequest> controllerInput = ReceiptItemDtoGenerator.GenerateUpdateRequestList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateReceiptItemCommand>(c => c.ReceiptItems.Count == controllerInput.Count && c.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<bool> result = await _controller.UpdateReceiptItems(controllerInput, receiptId);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task DeleteReceiptItems_ReturnsNoContent_WhenDeleteSucceeds()
	{
		// Arrange
		List<Guid> controllerInput = [.. ReceiptItemGenerator.GenerateList(2).Select(a => a.Id)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteReceiptItemCommand>(c => c.Ids.SequenceEqual(controllerInput)),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		ActionResult<bool> result = await _controller.DeleteReceiptItems(controllerInput);

		// Assert
		Assert.IsType<NoContentResult>(result.Result);
	}

	[Fact]
	public async Task DeleteReceiptItems_ReturnsNotFound_WhenDeleteFails()
	{
		// Arrange
		List<Guid> controllerInput = [.. ReceiptItemGenerator.GenerateList(2).Select(ri => ri.Id)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteReceiptItemCommand>(c => c.Ids.SequenceEqual(controllerInput)),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.DeleteReceiptItems(controllerInput);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task DeleteReceiptItems_ReturnsNotFound_WhenMultipleReceiptItemsDeleteFails()
	{
		// Arrange
		List<Guid> controllerInput = [.. ReceiptItemGenerator.GenerateList(2).Select(ri => ri.Id)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteReceiptItemCommand>(c => c.Ids.SequenceEqual(controllerInput)),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.DeleteReceiptItems(controllerInput);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task DeleteReceiptItems_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		List<Guid> controllerInput = [.. ReceiptItemGenerator.GenerateList(2).Select(ri => ri.Id)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteReceiptItemCommand>(c => c.Ids.SequenceEqual(controllerInput)),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<bool> result = await _controller.DeleteReceiptItems(controllerInput);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}
}
