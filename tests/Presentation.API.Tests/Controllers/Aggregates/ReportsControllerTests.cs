using API.Controllers.Aggregates;
using API.Generated.Dtos;
using Application.Queries.Aggregates.Reports;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using AppReports = Application.Models.Reports;

namespace Presentation.API.Tests.Controllers.Aggregates;

public class ReportsControllerTests
{
	private readonly Mock<IMediator> _mediatorMock;
	private readonly ReportsController _controller;

	public ReportsControllerTests()
	{
		_mediatorMock = new Mock<IMediator>();
		_controller = new ReportsController(_mediatorMock.Object);
	}

	[Fact]
	public async Task GetOutOfBalance_ReturnsOkResult_WithDefaultParameters()
	{
		// Arrange
		AppReports.OutOfBalanceResult reportResult = new(
		[
			new AppReports.OutOfBalanceItem(
				Guid.NewGuid(), "Store A", new DateOnly(2025, 3, 1),
				10.00m, 1.00m, 0m, 11.00m, 15.00m, -4.00m),
		], 1, 4.00m);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetOutOfBalanceReportQuery>(q =>
				q.SortBy == "date" && q.SortDirection == "asc" && q.Page == 1 && q.PageSize == 50),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(reportResult);

		// Act
		Results<Ok<OutOfBalanceResponse>, BadRequest<string>> result =
			await _controller.GetOutOfBalance(null, null, null, null, CancellationToken.None);

		// Assert
		Ok<OutOfBalanceResponse> okResult = Assert.IsType<Ok<OutOfBalanceResponse>>(result.Result);
		OutOfBalanceResponse response = okResult.Value!;
		response.TotalCount.Should().Be(1);
		response.TotalDiscrepancy.Should().Be(4.00);
		response.Items.Should().ContainSingle();
		response.Items.First().Location.Should().Be("Store A");
		response.Items.First().Difference.Should().Be(-4.00);
	}

	[Fact]
	public async Task GetOutOfBalance_PassesCustomParameters()
	{
		// Arrange
		AppReports.OutOfBalanceResult reportResult = new([], 0, 0m);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetOutOfBalanceReportQuery>(q =>
				q.SortBy == "difference" && q.SortDirection == "desc" && q.Page == 2 && q.PageSize == 25),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(reportResult);

		// Act
		Results<Ok<OutOfBalanceResponse>, BadRequest<string>> result =
			await _controller.GetOutOfBalance("difference", "desc", 2, 25, CancellationToken.None);

		// Assert
		Assert.IsType<Ok<OutOfBalanceResponse>>(result.Result);
		_mediatorMock.Verify(m => m.Send(
			It.Is<GetOutOfBalanceReportQuery>(q =>
				q.SortBy == "difference" && q.SortDirection == "desc" && q.Page == 2 && q.PageSize == 25),
			It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task GetOutOfBalance_ReturnsBadRequest_WhenInvalidSortBy()
	{
		// Act
		Results<Ok<OutOfBalanceResponse>, BadRequest<string>> result =
			await _controller.GetOutOfBalance("invalid", null, null, null, CancellationToken.None);

		// Assert
		BadRequest<string> badResult = Assert.IsType<BadRequest<string>>(result.Result);
		badResult.Value.Should().Contain("Invalid sortBy");
	}

	[Fact]
	public async Task GetOutOfBalance_ReturnsBadRequest_WhenInvalidSortDirection()
	{
		// Act
		Results<Ok<OutOfBalanceResponse>, BadRequest<string>> result =
			await _controller.GetOutOfBalance(null, "invalid", null, null, CancellationToken.None);

		// Assert
		BadRequest<string> badResult = Assert.IsType<BadRequest<string>>(result.Result);
		badResult.Value.Should().Contain("Invalid sortDirection");
	}

	[Fact]
	public async Task GetOutOfBalance_ReturnsBadRequest_WhenPageLessThanOne()
	{
		// Act
		Results<Ok<OutOfBalanceResponse>, BadRequest<string>> result =
			await _controller.GetOutOfBalance(null, null, 0, null, CancellationToken.None);

		// Assert
		BadRequest<string> badResult = Assert.IsType<BadRequest<string>>(result.Result);
		badResult.Value.Should().Be("page must be at least 1");
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	[InlineData(101)]
	public async Task GetOutOfBalance_ReturnsBadRequest_WhenPageSizeOutOfRange(int pageSize)
	{
		// Act
		Results<Ok<OutOfBalanceResponse>, BadRequest<string>> result =
			await _controller.GetOutOfBalance(null, null, null, pageSize, CancellationToken.None);

		// Assert
		BadRequest<string> badResult = Assert.IsType<BadRequest<string>>(result.Result);
		badResult.Value.Should().Be("pageSize must be between 1 and 100");
	}

	[Theory]
	[InlineData("date")]
	[InlineData("difference")]
	public async Task GetOutOfBalance_AcceptsValidSortColumns(string sortBy)
	{
		// Arrange
		AppReports.OutOfBalanceResult reportResult = new([], 0, 0m);

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<GetOutOfBalanceReportQuery>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(reportResult);

		// Act
		Results<Ok<OutOfBalanceResponse>, BadRequest<string>> result =
			await _controller.GetOutOfBalance(sortBy, null, null, null, CancellationToken.None);

		// Assert
		Assert.IsType<Ok<OutOfBalanceResponse>>(result.Result);
	}

	[Fact]
	public async Task GetOutOfBalance_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		_mediatorMock.Setup(m => m.Send(
			It.IsAny<GetOutOfBalanceReportQuery>(),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.GetOutOfBalance(null, null, null, null, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task GetOutOfBalance_MapsResponseFieldsCorrectly()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		DateOnly date = new(2025, 6, 15);

		AppReports.OutOfBalanceResult reportResult = new(
		[
			new AppReports.OutOfBalanceItem(
				receiptId, "Test Location", date,
				25.50m, 2.25m, 1.00m, 28.75m, 30.00m, -1.25m),
		], 1, 1.25m);

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<GetOutOfBalanceReportQuery>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(reportResult);

		// Act
		Results<Ok<OutOfBalanceResponse>, BadRequest<string>> result =
			await _controller.GetOutOfBalance(null, null, null, null, CancellationToken.None);

		// Assert
		Ok<OutOfBalanceResponse> okResult = Assert.IsType<Ok<OutOfBalanceResponse>>(result.Result);
		OutOfBalanceItem item = okResult.Value!.Items.First();
		item.ReceiptId.Should().Be(receiptId);
		item.Location.Should().Be("Test Location");
		item.Date.Should().Be(date);
		item.ItemSubtotal.Should().Be(25.50);
		item.TaxAmount.Should().Be(2.25);
		item.AdjustmentTotal.Should().Be(1.00);
		item.ExpectedTotal.Should().Be(28.75);
		item.TransactionTotal.Should().Be(30.00);
		item.Difference.Should().Be(-1.25);
	}
}
