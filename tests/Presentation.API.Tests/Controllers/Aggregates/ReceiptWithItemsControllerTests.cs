using API.Controllers.Aggregates;
using API.Mapping.Aggregates;
using API.Mapping.Core;
using Application.Queries.Aggregates.ReceiptsWithItems;
using AutoMapper;
using Domain.Aggregates;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SampleData.Domain.Aggregates;
using Shared.ViewModels.Aggregates;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Presentation.API.Tests.Controllers.Aggregates;

public class ReceiptWithItemsControllerTests
{
	private readonly IMapper _mapper;
	private readonly Mock<IMediator> _mediatorMock;
	private readonly Mock<IMapper> _mapperMock;
	private readonly Mock<ILogger<ReceiptWithItemsController>> _loggerMock;
	private readonly ReceiptWithItemsController _controller;

	public ReceiptWithItemsControllerTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<ReceiptWithItemsMappingProfile>();
			cfg.AddProfile<ReceiptMappingProfile>();
			cfg.AddProfile<ReceiptItemMappingProfile>();
		}, NullLoggerFactory.Instance);

		_mapper = configuration.CreateMapper();
		_mediatorMock = new Mock<IMediator>();
		_mapperMock = ControllerTestHelpers.GetMapperMock<ReceiptWithItems, ReceiptWithItemsVM>(_mapper);
		_loggerMock = ControllerTestHelpers.GetLoggerMock<ReceiptWithItemsController>();
		_controller = new ReceiptWithItemsController(_mediatorMock.Object, _mapperMock.Object, _loggerMock.Object);
	}

	[Fact]
	public async Task GetReceiptWithItemsByReceiptId_ReturnsOkResult_WhenReceiptWithItemsExists()
	{
		// Arrange
		ReceiptWithItems receiptWithItems = ReceiptWithItemsGenerator.Generate();
		ReceiptWithItemsVM expectedReturn = _mapper.Map<ReceiptWithItemsVM>(receiptWithItems);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptWithItemsByReceiptIdQuery>(q => q.ReceiptId == receiptWithItems.Receipt.Id!.Value),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(receiptWithItems);

		// Act
		ActionResult<ReceiptWithItemsVM> result = await _controller.GetReceiptWithItemsByReceiptId(receiptWithItems.Receipt.Id!.Value);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		ReceiptWithItemsVM actualReturn = Assert.IsType<ReceiptWithItemsVM>(okResult.Value);

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
		ActionResult<ReceiptWithItemsVM> result = await _controller.GetReceiptWithItemsByReceiptId(missingReceiptId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task GetReceiptWithItemsByReceiptId_ReturnsInternalServerError_WhenExceptionIsThrown()
	{
		// Arrange
		Guid receiptId = ReceiptWithItemsGenerator.Generate().Receipt.Id!.Value;

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptWithItemsByReceiptIdQuery>(q => q.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		ActionResult<ReceiptWithItemsVM> result = await _controller.GetReceiptWithItemsByReceiptId(receiptId);

		// Assert
		Assert.IsType<ObjectResult>(result.Result);
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(500, objectResult.StatusCode);
	}
}
