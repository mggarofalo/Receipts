using API.Controllers.Aggregates;
using API.Mapping.Aggregates;
using API.Mapping.Core;
using Application.Queries.Aggregates.Trips;
using Domain.Aggregates;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SampleData.Domain.Aggregates;
using Shared.ViewModels.Aggregates;
using FluentAssertions;

namespace Presentation.API.Tests.Controllers.Aggregates;

public class TripControllerTests
{
	private readonly TripMapper _mapper;
	private readonly Mock<IMediator> _mediatorMock;
	private readonly Mock<ILogger<TripController>> _loggerMock;
	private readonly TripController _controller;

	public TripControllerTests()
	{
		_mediatorMock = new Mock<IMediator>();
		_mapper = new TripMapper();
		_loggerMock = ControllerTestHelpers.GetLoggerMock<TripController>();
		_controller = new TripController(_mediatorMock.Object, _mapper, _loggerMock.Object);
	}

	[Fact]
	public async Task GetTripByReceiptId_ReturnsOkResult_WhenReceiptExists()
	{
		// Arrange
		Trip trip = TripGenerator.Generate();
		TripVM expectedReturn = _mapper.ToViewModel(trip);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTripByReceiptIdQuery>(q => q.ReceiptId == trip.Receipt.Receipt.Id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(trip);

		// Act
		ActionResult<TripVM> result = await _controller.GetTripByReceiptId(trip.Receipt.Receipt.Id);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		TripVM actualReturn = Assert.IsType<TripVM>(okResult.Value);

		actualReturn.Should().BeEquivalentTo(expectedReturn);
	}

	[Fact]
	public async Task GetTripByReceiptId_ReturnsNotFound_WhenReceiptDoesNotExist()
	{
		// Arrange
		Guid missingReceiptId = Guid.NewGuid();

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTripByReceiptIdQuery>(q => q.ReceiptId == missingReceiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync((Trip?)null);

		// Act
		ActionResult<TripVM> result = await _controller.GetTripByReceiptId(missingReceiptId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task GetTripByReceiptId_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		Guid receiptId = TripGenerator.Generate().Receipt.Receipt.Id;

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTripByReceiptIdQuery>(q => q.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<TripVM> result = await _controller.GetTripByReceiptId(receiptId);

		// Assert
		Assert.IsType<ObjectResult>(result.Result);
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}
}
