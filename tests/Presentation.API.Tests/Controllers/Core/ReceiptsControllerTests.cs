using API.Controllers.Core;
using API.Generated.Dtos;
using API.Mapping.Core;
using API.Services;
using Application.Commands.Receipt.Create;
using Application.Commands.Receipt.Delete;
using Application.Commands.Receipt.Restore;
using Application.Commands.Receipt.Update;
using Application.Models;
using Application.Queries.Core.Receipt;
using Domain.Core;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
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
	private readonly Mock<IEntityChangeNotifier> _notifierMock;
	private readonly ReceiptsController _controller;

	public ReceiptsControllerTests()
	{
		_mediatorMock = new Mock<IMediator>();
		_mapper = new ReceiptMapper();
		_loggerMock = ControllerTestHelpers.GetLoggerMock<ReceiptsController>();
		_notifierMock = new Mock<IEntityChangeNotifier>();

		_controller = new ReceiptsController(_mediatorMock.Object, _mapper, _loggerMock.Object, _notifierMock.Object);
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
		Results<Ok<ReceiptResponse>, NotFound> result = await _controller.GetReceiptById(mediatorReturn.Id);

		// Assert
		Ok<ReceiptResponse> okResult = Assert.IsType<Ok<ReceiptResponse>>(result.Result);
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
		Results<Ok<ReceiptResponse>, NotFound> result = await _controller.GetReceiptById(missingReceiptId);

		// Assert
		Assert.IsType<NotFound>(result.Result);
	}

	[Fact]
	public async Task GetReceiptById_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		Guid id = ReceiptGenerator.Generate().Id;

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetReceiptByIdQuery>(q => q.Id == id),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.GetReceiptById(id);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task GetAllReceipts_ReturnsOkResult_WithListOfReceipts()
	{
		// Arrange
		List<Receipt> mediatorReturn = ReceiptGenerator.GenerateList(2);
		List<ReceiptResponse> expectedControllerReturn = [.. mediatorReturn.Select(_mapper.ToResponse)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAllReceiptsQuery>(q => q.Offset == 0 && q.Limit == 50),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(new PagedResult<Receipt>(mediatorReturn, mediatorReturn.Count, 0, 50));

		// Act
		Ok<ReceiptListResponse> result = await _controller.GetAllReceipts(0, 50);

		// Assert
		ReceiptListResponse actualControllerReturn = result.Value!;

		actualControllerReturn.Data.Should().BeEquivalentTo(expectedControllerReturn);
		actualControllerReturn.Total.Should().Be(mediatorReturn.Count);
		actualControllerReturn.Offset.Should().Be(0);
		actualControllerReturn.Limit.Should().Be(50);
	}

	[Fact]
	public async Task GetAllReceipts_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAllReceiptsQuery>(q => q.Offset == 0 && q.Limit == 50),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.GetAllReceipts(0, 50);

		// Assert
		await act.Should().ThrowAsync<Exception>();
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
		Ok<ReceiptResponse> result = await _controller.CreateReceipt(controllerInput);

		// Assert
		ReceiptResponse actualReturn = result.Value!;

		actualReturn.Should().BeEquivalentTo(expectedReturn);
		_notifierMock.Verify(n => n.NotifyCreated("receipt", receipt.Id), Times.Once);
	}

	[Fact]
	public async Task CreateReceipt_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		CreateReceiptRequest controllerInput = ReceiptDtoGenerator.GenerateCreateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateReceiptCommand>(c => c.Receipts.Count == 1),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.CreateReceipt(controllerInput);

		// Assert
		await act.Should().ThrowAsync<Exception>();
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
		Ok<List<ReceiptResponse>> result = await _controller.CreateReceipts(controllerInput);

		// Assert
		List<ReceiptResponse> actualControllerReturn = result.Value!;

		actualControllerReturn.Should().BeEquivalentTo(expectedControllerReturn);
		_notifierMock.Verify(n => n.NotifyBulkChanged("receipt", "created", It.IsAny<IEnumerable<Guid>>()), Times.Once);
	}

	[Fact]
	public async Task CreateReceipts_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		List<CreateReceiptRequest> controllerInput = ReceiptDtoGenerator.GenerateCreateRequestList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateReceiptCommand>(c => c.Receipts.Count == controllerInput.Count),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.CreateReceipts(controllerInput);

		// Assert
		await act.Should().ThrowAsync<Exception>();
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
		Results<NoContent, NotFound> result = await _controller.UpdateReceipt(controllerInput.Id, controllerInput);

		// Assert
		Assert.IsType<NoContent>(result.Result);
		_notifierMock.Verify(n => n.NotifyUpdated("receipt", controllerInput.Id), Times.Once);
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
		Results<NoContent, NotFound> result = await _controller.UpdateReceipt(controllerInput.Id, controllerInput);

		// Assert
		Assert.IsType<NotFound>(result.Result);
	}

	[Fact]
	public async Task UpdateReceipt_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		UpdateReceiptRequest controllerInput = ReceiptDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateReceiptCommand>(c => c.Receipts.Count == 1),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.UpdateReceipt(controllerInput.Id, controllerInput);

		// Assert
		await act.Should().ThrowAsync<Exception>();
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
		Results<NoContent, NotFound> result = await _controller.UpdateReceipts(controllerInput);

		// Assert
		Assert.IsType<NoContent>(result.Result);
		_notifierMock.Verify(n => n.NotifyBulkChanged("receipt", "updated", It.IsAny<IEnumerable<Guid>>()), Times.Once);
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
		Results<NoContent, NotFound> result = await _controller.UpdateReceipts(controllerInput);

		// Assert
		Assert.IsType<NotFound>(result.Result);
	}

	[Fact]
	public async Task UpdateReceipts_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		List<UpdateReceiptRequest> controllerInput = ReceiptDtoGenerator.GenerateUpdateRequestList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateReceiptCommand>(c => c.Receipts.Count == controllerInput.Count),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.UpdateReceipts(controllerInput);

		// Assert
		await act.Should().ThrowAsync<Exception>();
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
		Results<NoContent, NotFound> result = await _controller.DeleteReceipts(controllerInput);

		// Assert
		Assert.IsType<NoContent>(result.Result);
		_notifierMock.Verify(n => n.NotifyBulkChanged("receipt", "deleted", controllerInput), Times.Once);
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
		Results<NoContent, NotFound> result = await _controller.DeleteReceipts(controllerInput);

		// Assert
		Assert.IsType<NotFound>(result.Result);
	}

	[Fact]
	public async Task DeleteReceipts_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		List<Guid> controllerInput = [.. ReceiptGenerator.GenerateList(2).Select(a => a.Id)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteReceiptCommand>(c => c.Ids.SequenceEqual(controllerInput)),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.DeleteReceipts(controllerInput);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task RestoreReceipt_ReturnsNoContent_WhenSuccessful()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		_mediatorMock.Setup(m => m.Send(
			It.Is<RestoreReceiptCommand>(c => c.Id == id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		Results<NoContent, NotFound> result = await _controller.RestoreReceipt(id);

		// Assert
		Assert.IsType<NoContent>(result.Result);
		_notifierMock.Verify(n => n.NotifyUpdated("receipt", id), Times.Once);
	}

	[Fact]
	public async Task RestoreReceipt_ReturnsNotFound_WhenEntityDoesNotExist()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		_mediatorMock.Setup(m => m.Send(
			It.Is<RestoreReceiptCommand>(c => c.Id == id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		Results<NoContent, NotFound> result = await _controller.RestoreReceipt(id);

		// Assert
		Assert.IsType<NotFound>(result.Result);
	}

	[Fact]
	public async Task RestoreReceipt_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		_mediatorMock.Setup(m => m.Send(
			It.Is<RestoreReceiptCommand>(c => c.Id == id),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.RestoreReceipt(id);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}
}
