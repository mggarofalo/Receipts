using API.Controllers.Core;
using API.Mapping.Core;
using Application.Commands.ReceiptItem;
using Application.Queries.Core.ReceiptItem;
using AutoMapper;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SampleData.Domain.Core;
using SampleData.ViewModels.Core;
using Shared.ViewModels.Core;

namespace Presentation.API.Tests.Controllers.Core;

public class ReceiptItemsControllerTests
{
	private readonly IMapper _mapper;
	private readonly Mock<IMediator> _mediatorMock;
	private readonly Mock<IMapper> _mapperMock;
	private readonly Mock<ILogger<ReceiptItemsController>> _loggerMock;
	private readonly ReceiptItemsController _controller;

	public ReceiptItemsControllerTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<ReceiptItemMappingProfile>();
		});

		_mapper = configuration.CreateMapper();
		_mediatorMock = new Mock<IMediator>();
		_mapperMock = ControllerTestHelpers.GetMapperMock<ReceiptItem, ReceiptItemVM>(_mapper);
		_loggerMock = ControllerTestHelpers.GetLoggerMock<ReceiptItemsController>();
		_controller = new ReceiptItemsController(_mediatorMock.Object, _mapperMock.Object, _loggerMock.Object);
	}

	[Fact]
	public async Task GetReceiptItemById_ReturnsOkResult_WhenReceiptItemExists()
	{
		// Arrange
		ReceiptItem receiptItem = ReceiptItemGenerator.Generate();
		ReceiptItemVM expectedReturn = _mapper.Map<ReceiptItemVM>(receiptItem);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptItemByIdQuery>(q => q.Id == receiptItem.Id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(receiptItem);

		// Act
		ActionResult<ReceiptItemVM> result = await _controller.GetReceiptItemById(receiptItem.Id!.Value);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		ReceiptItemVM actualReturn = Assert.IsType<ReceiptItemVM>(okResult.Value);
		Assert.Equal(expectedReturn, actualReturn);
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
		ActionResult<ReceiptItemVM> result = await _controller.GetReceiptItemById(missingReceiptItemId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task GetReceiptItemById_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		Guid id = ReceiptItemGenerator.Generate().Id!.Value;

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptItemByIdQuery>(q => q.Id == id),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<ReceiptItemVM> result = await _controller.GetReceiptItemById(id);

		// Assert
		Assert.IsType<ObjectResult>(result.Result);
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task GetAllReceiptItems_ReturnsOkResult_WithListOfReceiptItems()
	{
		// Arrange
		List<ReceiptItem> receiptItems = ReceiptItemGenerator.GenerateList(2);
		List<ReceiptItemVM> expectedReturn = _mapper.Map<List<ReceiptItemVM>>(receiptItems);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAllReceiptItemsQuery>(q => true),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(receiptItems);

		// Act
		ActionResult<List<ReceiptItemVM>> result = await _controller.GetAllReceiptItems();

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<ReceiptItemVM> actualReturn = Assert.IsType<List<ReceiptItemVM>>(okResult.Value);

		Assert.Equal(expectedReturn, actualReturn);
	}

	[Fact]
	public async Task GetAllAccounts_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAllReceiptItemsQuery>(q => true),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<List<ReceiptItemVM>> result = await _controller.GetAllReceiptItems();

		// Assert
		Assert.IsType<ObjectResult>(result.Result);
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task GetReceiptItemsByReceiptId_ReturnsOkResult_WithReceiptItems()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<ReceiptItem> receiptItems = ReceiptItemGenerator.GenerateList(2);
		List<ReceiptItemVM> expectedReturn = _mapper.Map<List<ReceiptItemVM>>(receiptItems);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptItemsByReceiptIdQuery>(q => q.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(receiptItems);

		// Act
		ActionResult<List<ReceiptItemVM>?> result = await _controller.GetReceiptItemsByReceiptId(receiptId);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<ReceiptItemVM> actualReturn = Assert.IsType<List<ReceiptItemVM>>(okResult.Value);

		Assert.Equal(expectedReturn, actualReturn);
	}

	[Fact]
	public async Task GetReceiptItemsByReceiptId_ReturnsOkResult_WithEmptyList_WhenReceiptFoundButNoItems()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<ReceiptItem> receiptItems = [];
		List<ReceiptItemVM> expectedReturn = _mapper.Map<List<ReceiptItemVM>>(receiptItems);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptItemsByReceiptIdQuery>(q => q.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(receiptItems);

		// Act
		ActionResult<List<ReceiptItemVM>?> result = await _controller.GetReceiptItemsByReceiptId(receiptId);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<ReceiptItemVM> actualReturn = Assert.IsType<List<ReceiptItemVM>>(okResult.Value);

		Assert.Equal(expectedReturn, actualReturn);
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
		ActionResult<List<ReceiptItemVM>?> result = await _controller.GetReceiptItemsByReceiptId(missingReceiptId);

		// Assert
		NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
		Assert.Equal(404, notFoundResult.StatusCode);
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
		ActionResult<List<ReceiptItemVM>?> result = await _controller.GetReceiptItemsByReceiptId(receiptId);

		// Assert
		Assert.IsType<ObjectResult>(result.Result);
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task CreateReceiptItems_ReturnsOkResult_WithCreatedReceiptItems()
	{
		// Arrange
		List<ReceiptItem> receiptItems = ReceiptItemGenerator.GenerateList(2);
		List<ReceiptItemVM> expectedReturn = _mapper.Map<List<ReceiptItemVM>>(receiptItems);

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateReceiptItemCommand>(c => c.ReceiptItems.Count == receiptItems.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(receiptItems);

		List<ReceiptItemVM> models = _mapper.Map<List<ReceiptItemVM>>(receiptItems);
		models.ForEach(a => a.Id = null);

		// Act
		ActionResult<ReceiptItemVM> result = await _controller.CreateReceiptItems(models);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		List<ReceiptItemVM> actualReturn = Assert.IsType<List<ReceiptItemVM>>(okResult.Value);

		Assert.Equal(expectedReturn, actualReturn);
	}

	[Fact]
	public async Task CreateReceiptItems_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		List<ReceiptItemVM> models = ReceiptItemVMGenerator.GenerateList(2);
		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateReceiptItemCommand>(c => c.ReceiptItems.Count == models.Count),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<ReceiptItemVM> result = await _controller.CreateReceiptItems(models);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task UpdateReceiptItems_ReturnsNoContent_WhenUpdateSucceeds()
	{
		// Arrange
		List<ReceiptItemVM> models = ReceiptItemVMGenerator.GenerateList(2);
		List<ReceiptItem> receiptItems = _mapper.Map<List<ReceiptItem>>(models);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateReceiptItemCommand>(c => c.ReceiptItems.Count == models.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		ActionResult<bool> result = await _controller.UpdateReceiptItems(models);

		// Assert
		NoContentResult noContentResult = Assert.IsType<NoContentResult>(result.Result);
		Assert.Equal(204, noContentResult.StatusCode);
	}

	[Fact]
	public async Task UpdateReceiptItems_ReturnsNotFound_WhenUpdateFails()
	{
		// Arrange
		List<ReceiptItemVM> models = ReceiptItemVMGenerator.GenerateList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateReceiptItemCommand>(c => c.ReceiptItems.Count == models.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.UpdateReceiptItems(models);

		// Assert
		NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
		Assert.Equal(404, notFoundResult.StatusCode);
	}

	[Fact]
	public async Task UpdateReceiptItems_ReturnsInternalServerError_WhenExceptionThrown()
	{
		// Arrange
		List<ReceiptItemVM> models = ReceiptItemVMGenerator.GenerateList(2);
		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateReceiptItemCommand>(c => c.ReceiptItems.Count == models.Count),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<bool> result = await _controller.UpdateReceiptItems(models);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}

	[Fact]
	public async Task DeleteReceiptItems_ReturnsNoContent_WhenDeleteSucceeds()
	{
		// Arrange
		List<Guid> ids = ReceiptItemGenerator.GenerateList(2).Select(a => a.Id!.Value).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteReceiptItemCommand>(c => c.Ids.SequenceEqual(ids)),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		ActionResult<bool> result = await _controller.DeleteReceiptItems(ids);

		// Assert
		NoContentResult noContentResult = Assert.IsType<NoContentResult>(result.Result);
		Assert.Equal(204, noContentResult.StatusCode);
	}

	[Fact]
	public async Task DeleteReceiptItems_ReturnsNotFound_WhenSingleReceiptItemDeleteFails()
	{
		// Arrange
		List<Guid> ids = [AccountGenerator.Generate().Id!.Value];

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<DeleteReceiptItemCommand>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.DeleteReceiptItems(ids);

		// Assert
		NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
		Assert.Equal(404, notFoundResult.StatusCode);
	}

	[Fact]
	public async Task DeleteReceiptItems_ReturnsNotFound_WhenMultipleReceiptItemsDeleteFails()
	{
		// Arrange
		List<Guid> ids = AccountGenerator.GenerateList(2).Select(a => a.Id!.Value).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<DeleteReceiptItemCommand>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.DeleteReceiptItems(ids);

		// Assert
		NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
		Assert.Equal(404, notFoundResult.StatusCode);
	}

	[Fact]
	public async Task DeleteReceiptItems_ReturnsInternalServerError_WhenExceptionThrown()
	{
		// Arrange
		List<Guid> ids = AccountGenerator.GenerateList(2).Select(a => a.Id!.Value).ToList();

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<DeleteReceiptItemCommand>(),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<bool> result = await _controller.DeleteReceiptItems(ids);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
		Assert.Equal("An error occurred while processing your request.", objectResult.Value);
	}
}