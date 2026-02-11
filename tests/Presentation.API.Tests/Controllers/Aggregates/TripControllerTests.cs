using API.Controllers.Aggregates;
using API.Mapping.Aggregates;
using API.Mapping.Core;
using Application.Queries.Aggregates.Trips;
using AutoMapper;
using Domain.Aggregates;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SampleData.Domain.Aggregates;
using Shared.ViewModels.Aggregates;
using Microsoft.Extensions.Logging.Abstractions;

namespace Presentation.API.Tests.Controllers.Aggregates;

public class TripControllerTests
{
	private readonly IMapper _mapper;
	private readonly Mock<IMediator> _mediatorMock;
	private readonly Mock<IMapper> _mapperMock;
	private readonly Mock<ILogger<TripController>> _loggerMock;
	private readonly TripController _controller;

	public TripControllerTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<TripMappingProfile>();
			cfg.AddProfile<ReceiptWithItemsMappingProfile>();
			cfg.AddProfile<TransactionAccountMappingProfile>();
			cfg.AddProfile<ReceiptMappingProfile>();
			cfg.AddProfile<ReceiptItemMappingProfile>();
			cfg.AddProfile<TransactionMappingProfile>();
			cfg.AddProfile<AccountMappingProfile>();
		}, NullLoggerFactory.Instance);

		_mapper = configuration.CreateMapper();
		_mediatorMock = new Mock<IMediator>();
		_mapperMock = ControllerTestHelpers.GetMapperMock<Trip, TripVM>(_mapper);
		_loggerMock = ControllerTestHelpers.GetLoggerMock<TripController>();
		_controller = new TripController(_mediatorMock.Object, _mapperMock.Object, _loggerMock.Object);
	}

	[Fact]
	public async Task GetTripByReceiptId_ReturnsOkResult_WhenReceiptExists()
	{
		// Arrange
		Trip trip = TripGenerator.Generate();
		TripVM expectedReturn = _mapper.Map<TripVM>(trip);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetTripByReceiptIdQuery>(q => q.ReceiptId == trip.Receipt.Receipt.Id!.Value),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(trip);

		// Act
		ActionResult<TripVM> result = await _controller.GetTripByReceiptId(trip.Receipt.Receipt.Id!.Value);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		TripVM actualReturn = Assert.IsType<TripVM>(okResult.Value);

		Assert.Equal(expectedReturn, actualReturn);
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
		Guid receiptId = TripGenerator.Generate().Receipt.Receipt.Id!.Value;

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
