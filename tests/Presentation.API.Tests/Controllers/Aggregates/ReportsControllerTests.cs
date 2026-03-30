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

	// ── GetItemDescriptions ──────────────────────────────

	[Fact]
	public async Task GetItemDescriptions_ReturnsOkResult_WithValidSearch()
	{
		// Arrange
		AppReports.ItemDescriptionResult descResult = new(
		[
			new AppReports.ItemDescriptionItem("Milk", "Dairy", 10),
			new AppReports.ItemDescriptionItem("Milk Chocolate", "Candy", 3),
		]);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetItemDescriptionsQuery>(q =>
				q.Search == "mi" && !q.CategoryOnly && q.Limit == 20),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(descResult);

		// Act
		Results<Ok<ItemDescriptionsResponse>, BadRequest<string>> result =
			await _controller.GetItemDescriptions("mi", null, null, CancellationToken.None);

		// Assert
		Ok<ItemDescriptionsResponse> okResult = Assert.IsType<Ok<ItemDescriptionsResponse>>(result.Result);
		okResult.Value!.Items.Should().HaveCount(2);
		okResult.Value!.Items.First().Description.Should().Be("Milk");
		okResult.Value!.Items.First().Category.Should().Be("Dairy");
		okResult.Value!.Items.First().Occurrences.Should().Be(10);
	}

	[Fact]
	public async Task GetItemDescriptions_PassesCategoryOnlyAndLimit()
	{
		// Arrange
		AppReports.ItemDescriptionResult descResult = new([]);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetItemDescriptionsQuery>(q =>
				q.Search == "da" && q.CategoryOnly && q.Limit == 10),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(descResult);

		// Act
		Results<Ok<ItemDescriptionsResponse>, BadRequest<string>> result =
			await _controller.GetItemDescriptions("da", true, 10, CancellationToken.None);

		// Assert
		Assert.IsType<Ok<ItemDescriptionsResponse>>(result.Result);
		_mediatorMock.Verify(m => m.Send(
			It.Is<GetItemDescriptionsQuery>(q =>
				q.Search == "da" && q.CategoryOnly && q.Limit == 10),
			It.IsAny<CancellationToken>()), Times.Once);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("a")]
	public async Task GetItemDescriptions_ReturnsBadRequest_WhenSearchTooShort(string? search)
	{
		// Act
		Results<Ok<ItemDescriptionsResponse>, BadRequest<string>> result =
			await _controller.GetItemDescriptions(search, null, null, CancellationToken.None);

		// Assert
		BadRequest<string> badResult = Assert.IsType<BadRequest<string>>(result.Result);
		badResult.Value.Should().Contain("search must be at least 2 characters");
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	[InlineData(51)]
	public async Task GetItemDescriptions_ReturnsBadRequest_WhenLimitOutOfRange(int limit)
	{
		// Act
		Results<Ok<ItemDescriptionsResponse>, BadRequest<string>> result =
			await _controller.GetItemDescriptions("milk", null, limit, CancellationToken.None);

		// Assert
		BadRequest<string> badResult = Assert.IsType<BadRequest<string>>(result.Result);
		badResult.Value.Should().Contain("limit must be between 1 and 50");
	}

	// ── GetItemCostOverTime ──────────────────────────────

	[Fact]
	public async Task GetItemCostOverTime_ReturnsOkResult_WithDescription()
	{
		// Arrange
		AppReports.ItemCostOverTimeResult costResult = new(
		[
			new AppReports.ItemCostBucket("2025-01-15", 3.99m),
			new AppReports.ItemCostBucket("2025-02-20", 4.29m),
		]);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetItemCostOverTimeQuery>(q =>
				q.Description == "Milk" && q.Category == null && q.Granularity == "exact"),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(costResult);

		// Act
		Results<Ok<ItemCostOverTimeResponse>, BadRequest<string>> result =
			await _controller.GetItemCostOverTime("Milk", null, null, null, null, CancellationToken.None);

		// Assert
		Ok<ItemCostOverTimeResponse> okResult = Assert.IsType<Ok<ItemCostOverTimeResponse>>(result.Result);
		okResult.Value!.Buckets.Should().HaveCount(2);
		okResult.Value!.Buckets.First().Period.Should().Be("2025-01-15");
		okResult.Value!.Buckets.First().Amount.Should().Be(3.99);
	}

	[Fact]
	public async Task GetItemCostOverTime_ReturnsOkResult_WithCategory()
	{
		// Arrange
		AppReports.ItemCostOverTimeResult costResult = new([]);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetItemCostOverTimeQuery>(q =>
				q.Description == null && q.Category == "Dairy" && q.Granularity == "monthly"),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(costResult);

		// Act
		Results<Ok<ItemCostOverTimeResponse>, BadRequest<string>> result =
			await _controller.GetItemCostOverTime(null, "Dairy", null, null, "monthly", CancellationToken.None);

		// Assert
		Assert.IsType<Ok<ItemCostOverTimeResponse>>(result.Result);
		_mediatorMock.Verify(m => m.Send(
			It.Is<GetItemCostOverTimeQuery>(q =>
				q.Category == "Dairy" && q.Granularity == "monthly"),
			It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task GetItemCostOverTime_ReturnsBadRequest_WhenNoDescriptionOrCategory()
	{
		// Act
		Results<Ok<ItemCostOverTimeResponse>, BadRequest<string>> result =
			await _controller.GetItemCostOverTime(null, null, null, null, null, CancellationToken.None);

		// Assert
		BadRequest<string> badResult = Assert.IsType<BadRequest<string>>(result.Result);
		badResult.Value.Should().Contain("Either description or category is required");
	}

	[Fact]
	public async Task GetItemCostOverTime_ReturnsBadRequest_WhenInvalidGranularity()
	{
		// Act
		Results<Ok<ItemCostOverTimeResponse>, BadRequest<string>> result =
			await _controller.GetItemCostOverTime("Milk", null, null, null, "invalid", CancellationToken.None);

		// Assert
		BadRequest<string> badResult = Assert.IsType<BadRequest<string>>(result.Result);
		badResult.Value.Should().Contain("Invalid granularity");
	}

	[Theory]
	[InlineData("exact")]
	[InlineData("monthly")]
	[InlineData("yearly")]
	public async Task GetItemCostOverTime_AcceptsValidGranularities(string granularity)
	{
		// Arrange
		AppReports.ItemCostOverTimeResult costResult = new([]);

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<GetItemCostOverTimeQuery>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(costResult);

		// Act
		Results<Ok<ItemCostOverTimeResponse>, BadRequest<string>> result =
			await _controller.GetItemCostOverTime("Milk", null, null, null, granularity, CancellationToken.None);

		// Assert
		Assert.IsType<Ok<ItemCostOverTimeResponse>>(result.Result);
	}

	[Fact]
	public async Task GetItemCostOverTime_ReturnsBadRequest_WhenStartDateAfterEndDate()
	{
		// Act
		Results<Ok<ItemCostOverTimeResponse>, BadRequest<string>> result =
			await _controller.GetItemCostOverTime("Milk", null, new DateOnly(2025, 12, 31), new DateOnly(2025, 1, 1), null, CancellationToken.None);

		// Assert
		BadRequest<string> badResult = Assert.IsType<BadRequest<string>>(result.Result);
		badResult.Value.Should().Contain("startDate must be before or equal to endDate");
	}

	[Fact]
	public async Task GetItemCostOverTime_PassesDateRange()
	{
		// Arrange
		DateOnly start = new(2025, 1, 1);
		DateOnly end = new(2025, 12, 31);
		AppReports.ItemCostOverTimeResult costResult = new([]);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetItemCostOverTimeQuery>(q =>
				q.StartDate == start && q.EndDate == end),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(costResult);

		// Act
		Results<Ok<ItemCostOverTimeResponse>, BadRequest<string>> result =
			await _controller.GetItemCostOverTime("Milk", null, start, end, null, CancellationToken.None);

		// Assert
		Assert.IsType<Ok<ItemCostOverTimeResponse>>(result.Result);
		_mediatorMock.Verify(m => m.Send(
			It.Is<GetItemCostOverTimeQuery>(q =>
				q.StartDate == start && q.EndDate == end),
			It.IsAny<CancellationToken>()), Times.Once);
	}

	// ── CategoryTrends ─────────────────────────────

	[Fact]
	public async Task GetCategoryTrends_ReturnsOkResult_WithDefaultParameters()
	{
		// Arrange
		AppReports.CategoryTrendsResult trendsResult = new(
			["Groceries", "Dining"],
			[new AppReports.CategoryTrendsBucketResult("2025-01", [100.00m, 50.00m])]);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetCategoryTrendsReportQuery>(q =>
				q.Granularity == "monthly" && q.TopN == 7),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(trendsResult);

		// Act
		Results<Ok<CategoryTrendsResponse>, BadRequest<string>> result =
			await _controller.GetCategoryTrends(null, null, null, null, CancellationToken.None);

		// Assert
		Ok<CategoryTrendsResponse> okResult = Assert.IsType<Ok<CategoryTrendsResponse>>(result.Result);
		CategoryTrendsResponse response = okResult.Value!;
		response.Categories.Should().ContainInOrder("Groceries", "Dining");
		response.Buckets.Should().ContainSingle();
		response.Buckets.First().Period.Should().Be("2025-01");
		response.Buckets.First().Amounts.Should().Equal(100.00, 50.00);
	}

	[Fact]
	public async Task GetCategoryTrends_ReturnsBadRequest_WhenStartDateAfterEndDate()
	{
		// Act
		Results<Ok<CategoryTrendsResponse>, BadRequest<string>> result =
			await _controller.GetCategoryTrends(new DateOnly(2025, 12, 31), new DateOnly(2025, 1, 1), null, null, CancellationToken.None);

		// Assert
		BadRequest<string> badResult = Assert.IsType<BadRequest<string>>(result.Result);
		badResult.Value.Should().Contain("startDate must be before or equal to endDate");
	}

	[Fact]
	public async Task GetCategoryTrends_ReturnsBadRequest_WhenInvalidGranularity()
	{
		// Act
		Results<Ok<CategoryTrendsResponse>, BadRequest<string>> result =
			await _controller.GetCategoryTrends(null, null, "invalid", null, CancellationToken.None);

		// Assert
		BadRequest<string> badResult = Assert.IsType<BadRequest<string>>(result.Result);
		badResult.Value.Should().Contain("Invalid granularity");
	}

	[Theory]
	[InlineData("daily")]
	[InlineData("monthly")]
	[InlineData("quarterly")]
	[InlineData("yearly")]
	public async Task GetCategoryTrends_AcceptsValidGranularities(string granularity)
	{
		// Arrange
		AppReports.CategoryTrendsResult trendsResult = new([], []);

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<GetCategoryTrendsReportQuery>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(trendsResult);

		// Act
		Results<Ok<CategoryTrendsResponse>, BadRequest<string>> result =
			await _controller.GetCategoryTrends(null, null, granularity, null, CancellationToken.None);

		// Assert
		Assert.IsType<Ok<CategoryTrendsResponse>>(result.Result);
	}

	[Fact]
	public async Task GetCategoryTrends_ReturnsBadRequest_WhenTopNTooLow()
	{
		// Act
		Results<Ok<CategoryTrendsResponse>, BadRequest<string>> result =
			await _controller.GetCategoryTrends(null, null, null, 0, CancellationToken.None);

		// Assert
		BadRequest<string> badResult = Assert.IsType<BadRequest<string>>(result.Result);
		badResult.Value.Should().Contain("topN must be between 1 and 50");
	}

	[Fact]
	public async Task GetCategoryTrends_ReturnsBadRequest_WhenTopNTooHigh()
	{
		// Act
		Results<Ok<CategoryTrendsResponse>, BadRequest<string>> result =
			await _controller.GetCategoryTrends(null, null, null, 51, CancellationToken.None);

		// Assert
		BadRequest<string> badResult = Assert.IsType<BadRequest<string>>(result.Result);
		badResult.Value.Should().Contain("topN must be between 1 and 50");
	}

	[Fact]
	public async Task GetCategoryTrends_PassesCustomParameters()
	{
		// Arrange
		DateOnly start = new(2024, 1, 1);
		DateOnly end = new(2024, 12, 31);
		AppReports.CategoryTrendsResult trendsResult = new([], []);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetCategoryTrendsReportQuery>(q =>
				q.StartDate == start && q.EndDate == end && q.Granularity == "quarterly" && q.TopN == 5),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(trendsResult);

		// Act
		Results<Ok<CategoryTrendsResponse>, BadRequest<string>> result =
			await _controller.GetCategoryTrends(start, end, "quarterly", 5, CancellationToken.None);

		// Assert
		Assert.IsType<Ok<CategoryTrendsResponse>>(result.Result);
		_mediatorMock.Verify(m => m.Send(
			It.Is<GetCategoryTrendsReportQuery>(q =>
				q.StartDate == start && q.EndDate == end && q.Granularity == "quarterly" && q.TopN == 5),
			It.IsAny<CancellationToken>()), Times.Once);
	}
}
