using API.Controllers.Core;
using API.Generated.Dtos;
using API.Mapping.Core;
using Application.Commands.Adjustment.Create;
using Application.Commands.Adjustment.Delete;
using Application.Commands.Adjustment.Restore;
using Application.Commands.Adjustment.Update;
using Application.Models;
using Application.Queries.Core.Adjustment;
using Domain.Core;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using API.Services;
using Microsoft.Extensions.Logging;
using Moq;
using SampleData.Domain.Core;
using SampleData.Dtos.Core;

namespace Presentation.API.Tests.Controllers.Core;

public class AdjustmentsControllerTests
{
	private readonly AdjustmentMapper _mapper;
	private readonly Mock<IMediator> _mediatorMock;
	private readonly Mock<ILogger<AdjustmentsController>> _loggerMock;
	private readonly Mock<IEntityChangeNotifier> _notifierMock;
	private readonly AdjustmentsController _controller;

	public AdjustmentsControllerTests()
	{
		_mediatorMock = new Mock<IMediator>();
		_mapper = new AdjustmentMapper();
		_loggerMock = ControllerTestHelpers.GetLoggerMock<AdjustmentsController>();
		_notifierMock = new Mock<IEntityChangeNotifier>();
		_controller = new AdjustmentsController(_mediatorMock.Object, _mapper, _loggerMock.Object, _notifierMock.Object);
	}

	[Fact]
	public async Task GetAdjustmentById_ReturnsOkResult_WhenAdjustmentExists()
	{
		// Arrange
		Adjustment adjustment = AdjustmentGenerator.Generate();
		AdjustmentResponse expectedReturn = _mapper.ToResponse(adjustment);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAdjustmentByIdQuery>(q => q.Id == adjustment.Id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(adjustment);

		// Act
		ActionResult<AdjustmentResponse> result = await _controller.GetAdjustmentById(adjustment.Id);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		AdjustmentResponse actualReturn = Assert.IsType<AdjustmentResponse>(okResult.Value);
		actualReturn.Should().BeEquivalentTo(expectedReturn);
	}

	[Fact]
	public async Task GetAdjustmentById_ReturnsNotFound_WhenAdjustmentDoesNotExist()
	{
		// Arrange
		Guid missingId = Guid.NewGuid();

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAdjustmentByIdQuery>(q => q.Id == missingId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync((Adjustment?)null);

		// Act
		ActionResult<AdjustmentResponse> result = await _controller.GetAdjustmentById(missingId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task GetAdjustmentById_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		Guid id = AdjustmentGenerator.Generate().Id;

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAdjustmentByIdQuery>(q => q.Id == id),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.GetAdjustmentById(id);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task GetAllAdjustments_ReturnsOkResult_WithListOfAdjustments()
	{
		// Arrange
		List<Adjustment> adjustments = AdjustmentGenerator.GenerateList(2);
		List<AdjustmentResponse> expectedReturn = [.. adjustments.Select(_mapper.ToResponse)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAllAdjustmentsQuery>(q => q.Offset == 0 && q.Limit == 50),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(new PagedResult<Adjustment>(adjustments, adjustments.Count, 0, 50));

		// Act
		ActionResult<AdjustmentListResponse> result = await _controller.GetAllAdjustments(null, 0, 50);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		AdjustmentListResponse actualReturn = Assert.IsType<AdjustmentListResponse>(okResult.Value);
		actualReturn.Data.Should().BeEquivalentTo(expectedReturn);
		actualReturn.Total.Should().Be(adjustments.Count);
		actualReturn.Offset.Should().Be(0);
		actualReturn.Limit.Should().Be(50);
	}

	[Fact]
	public async Task GetAllAdjustments_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAllAdjustmentsQuery>(q => q.Offset == 0 && q.Limit == 50),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.GetAllAdjustments(null, 0, 50);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task GetDeletedAdjustments_ReturnsOkResult_WithListOfAdjustments()
	{
		// Arrange
		List<Adjustment> adjustments = AdjustmentGenerator.GenerateList(2);
		List<AdjustmentResponse> expectedReturn = [.. adjustments.Select(_mapper.ToResponse)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetDeletedAdjustmentsQuery>(q => q.Offset == 0 && q.Limit == 50),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(new PagedResult<Adjustment>(adjustments, adjustments.Count, 0, 50));

		// Act
		ActionResult<AdjustmentListResponse> result = await _controller.GetDeletedAdjustments(0, 50);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		AdjustmentListResponse actualReturn = Assert.IsType<AdjustmentListResponse>(okResult.Value);
		actualReturn.Data.Should().BeEquivalentTo(expectedReturn);
		actualReturn.Total.Should().Be(adjustments.Count);
		actualReturn.Offset.Should().Be(0);
		actualReturn.Limit.Should().Be(50);
	}

	[Fact]
	public async Task GetDeletedAdjustments_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		_mediatorMock.Setup(m => m.Send(
			It.Is<GetDeletedAdjustmentsQuery>(q => q.Offset == 0 && q.Limit == 50),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.GetDeletedAdjustments(0, 50);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task GetAllAdjustments_WithReceiptId_ReturnsOkResult_WhenReceiptExists()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<Adjustment> adjustments = AdjustmentGenerator.GenerateList(2);
		List<AdjustmentResponse> expectedReturn = [.. adjustments.Select(_mapper.ToResponse)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAdjustmentsByReceiptIdQuery>(q => q.ReceiptId == receiptId && q.Offset == 0 && q.Limit == 50),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(new PagedResult<Adjustment>(adjustments, adjustments.Count, 0, 50));

		// Act
		ActionResult<AdjustmentListResponse> result = await _controller.GetAllAdjustments(receiptId, 0, 50);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		AdjustmentListResponse actualReturn = Assert.IsType<AdjustmentListResponse>(okResult.Value);
		actualReturn.Data.Should().BeEquivalentTo(expectedReturn);
		actualReturn.Total.Should().Be(adjustments.Count);
		actualReturn.Offset.Should().Be(0);
		actualReturn.Limit.Should().Be(50);
	}

	[Fact]
	public async Task GetAllAdjustments_WithReceiptId_ReturnsEmptyList_WhenReceiptDoesNotExist()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAdjustmentsByReceiptIdQuery>(q => q.ReceiptId == receiptId && q.Offset == 0 && q.Limit == 50),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(new PagedResult<Adjustment>([], 0, 0, 50));

		// Act
		ActionResult<AdjustmentListResponse> result = await _controller.GetAllAdjustments(receiptId, 0, 50);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		AdjustmentListResponse actualReturn = Assert.IsType<AdjustmentListResponse>(okResult.Value);
		actualReturn.Data.Should().BeEmpty();
	}

	[Fact]
	public async Task GetAllAdjustments_WithReceiptId_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAdjustmentsByReceiptIdQuery>(q => q.ReceiptId == receiptId && q.Offset == 0 && q.Limit == 50),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.GetAllAdjustments(receiptId, 0, 50);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task CreateAdjustment_ReturnsOkResult_WithCreatedAdjustment()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Adjustment adjustment = AdjustmentGenerator.Generate();
		AdjustmentResponse expectedReturn = _mapper.ToResponse(adjustment);

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateAdjustmentCommand>(c => c.Adjustments.Count == 1 && c.ReceiptId == receiptId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync([adjustment]);

		CreateAdjustmentRequest controllerInput = AdjustmentDtoGenerator.GenerateCreateRequest();

		// Act
		ActionResult<AdjustmentResponse> result = await _controller.CreateAdjustment(controllerInput, receiptId);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
		AdjustmentResponse actualReturn = Assert.IsType<AdjustmentResponse>(okResult.Value);
		actualReturn.Should().BeEquivalentTo(expectedReturn);
	}

	[Fact]
	public async Task CreateAdjustment_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		CreateAdjustmentRequest controllerInput = AdjustmentDtoGenerator.GenerateCreateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateAdjustmentCommand>(c => c.Adjustments.Count == 1),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.CreateAdjustment(controllerInput, receiptId);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task UpdateAdjustment_ReturnsNoContent_WhenUpdateSucceeds()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		UpdateAdjustmentRequest controllerInput = AdjustmentDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateAdjustmentCommand>(c => c.Adjustments.Count == 1),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		ActionResult<bool> result = await _controller.UpdateAdjustment(controllerInput, id);

		// Assert
		NoContentResult noContentResult = Assert.IsType<NoContentResult>(result.Result);
		Assert.Equal(204, noContentResult.StatusCode);
	}

	[Fact]
	public async Task UpdateAdjustment_ReturnsNotFound_WhenUpdateFails()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		UpdateAdjustmentRequest controllerInput = AdjustmentDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateAdjustmentCommand>(c => c.Adjustments.Count == 1),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.UpdateAdjustment(controllerInput, id);

		// Assert
		NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
		Assert.Equal(404, notFoundResult.StatusCode);
	}

	[Fact]
	public async Task UpdateAdjustment_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		UpdateAdjustmentRequest controllerInput = AdjustmentDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateAdjustmentCommand>(c => c.Adjustments.Count == 1),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.UpdateAdjustment(controllerInput, id);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task DeleteAdjustments_ReturnsNoContent_WhenDeleteSucceeds()
	{
		// Arrange
		List<Guid> ids = [.. AdjustmentGenerator.GenerateList(2).Select(a => a.Id)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteAdjustmentCommand>(c => c.Ids.SequenceEqual(ids)),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		ActionResult<bool> result = await _controller.DeleteAdjustments(ids);

		// Assert
		NoContentResult noContentResult = Assert.IsType<NoContentResult>(result.Result);
		Assert.Equal(204, noContentResult.StatusCode);
	}

	[Fact]
	public async Task DeleteAdjustments_ReturnsNotFound_WhenDeleteFails()
	{
		// Arrange
		List<Guid> ids = [AdjustmentGenerator.Generate().Id];

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteAdjustmentCommand>(c => c.Ids.SequenceEqual(ids)),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		ActionResult<bool> result = await _controller.DeleteAdjustments(ids);

		// Assert
		NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
		Assert.Equal(404, notFoundResult.StatusCode);
	}

	[Fact]
	public async Task DeleteAdjustments_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		List<Guid> ids = [.. AdjustmentGenerator.GenerateList(2).Select(a => a.Id)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteAdjustmentCommand>(c => c.Ids.SequenceEqual(ids)),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.DeleteAdjustments(ids);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task RestoreAdjustment_ReturnsNoContent_WhenSuccessful()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		_mediatorMock.Setup(m => m.Send(
			It.Is<RestoreAdjustmentCommand>(c => c.Id == id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		IActionResult result = await _controller.RestoreAdjustment(id);

		// Assert
		Assert.IsType<NoContentResult>(result);
	}

	[Fact]
	public async Task RestoreAdjustment_ReturnsNotFound_WhenEntityDoesNotExist()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		_mediatorMock.Setup(m => m.Send(
			It.Is<RestoreAdjustmentCommand>(c => c.Id == id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		IActionResult result = await _controller.RestoreAdjustment(id);

		// Assert
		Assert.IsType<NotFoundResult>(result);
	}

	[Fact]
	public async Task RestoreAdjustment_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		_mediatorMock.Setup(m => m.Send(
			It.Is<RestoreAdjustmentCommand>(c => c.Id == id),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.RestoreAdjustment(id);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}
}
