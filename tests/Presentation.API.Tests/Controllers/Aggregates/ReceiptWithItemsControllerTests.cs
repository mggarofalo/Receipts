using API.Controllers.Aggregates;
using API.Generated.Dtos;
using API.Mapping.Aggregates;
using API.Mapping.Core;
using Application.Queries.Aggregates.ReceiptsWithItems;
using Domain.Aggregates;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SampleData.Domain.Aggregates;

namespace Presentation.API.Tests.Controllers.Aggregates;

public class ReceiptWithItemsControllerTests
{
	private readonly ReceiptWithItemsMapper _mapper;
	private readonly Mock<IMediator> _mediatorMock;
	private readonly Mock<ILogger<ReceiptWithItemsController>> _loggerMock;
	private readonly ReceiptWithItemsController _controller;

	public ReceiptWithItemsControllerTests()
	{
		_mediatorMock = new Mock<IMediator>();
		_mapper = new ReceiptWithItemsMapper();
		_loggerMock = ControllerTestHelpers.GetLoggerMock<ReceiptWithItemsController>();
		_controller = new ReceiptWithItemsController(_mediatorMock.Object, _mapper, _loggerMock.Object);
	}

	[Fact]
	public async Task GetReceiptWithItemsByReceiptId_ReturnsOkResult_WhenReceiptWithItemsExists()
	{
		// Arrange
		ReceiptWithItems receiptWithItems = ReceiptWithItemsGenerator.Generate();
		ReceiptWithItemsResponse expectedReturn = _mapper.ToResponse(receiptWithItems);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptWithItemsByReceiptIdQuery>(q => q.ReceiptId == receiptWithItems.Receipt.Id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(receiptWithItems);

		// Act
		ActionResult<ReceiptWithItemsResponse> result = await _controller.GetReceiptWithItemsByReceiptId(receiptWithItems.Receipt.Id);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		ReceiptWithItemsResponse actualReturn = Assert.IsType<ReceiptWithItemsResponse>(okResult.Value);

		actualReturn.Should().BeEquivalentTo(expectedReturn);
	}

	[Fact]
	public async Task GetReceiptWithItemsByReceiptId_ReturnsNotFound_WhenReceiptWithItemsDoesNotExist()
	{
		// Arrange
		Guid missingReceiptId = Guid.NewGuid();

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptWithItemsByReceiptIdQuery>(q => q.ReceiptId == missingReceiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync((ReceiptWithItems?)null);

		// Act
		ActionResult<ReceiptWithItemsResponse> result = await _controller.GetReceiptWithItemsByReceiptId(missingReceiptId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task GetReceiptWithItemsByReceiptId_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		Guid receiptId = ReceiptWithItemsGenerator.Generate().Receipt.Id;

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptWithItemsByReceiptIdQuery>(q => q.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<ReceiptWithItemsResponse> result = await _controller.GetReceiptWithItemsByReceiptId(receiptId);

		// Assert
		Assert.IsType<ObjectResult>(result.Result);
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}
}
