using API.Controllers.Aggregates;
using API.Generated.Dtos;
using Application.Commands.Reports;
using Application.Queries.Aggregates.Reports;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using AppReports = Application.Models.Reports;

namespace Presentation.API.Tests.Controllers.Aggregates;

public class ItemSimilarityReportsControllerTests
{
	private readonly Mock<IMediator> _mediatorMock;
	private readonly ReportsController _controller;

	public ItemSimilarityReportsControllerTests()
	{
		_mediatorMock = new Mock<IMediator>();
		_controller = new ReportsController(_mediatorMock.Object);
	}

	[Fact]
	public async Task GetItemSimilarity_ReturnsOkResult_WithDefaultParameters()
	{
		// Arrange
		AppReports.ItemSimilarityResult reportResult = new(
		[
			new AppReports.ItemSimilarityGroup(
				"Milk", ["MILK", "Milk"], [Guid.NewGuid(), Guid.NewGuid()], 2, 0.85),
		], 1);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetItemSimilarityReportQuery>(q =>
				q.Threshold == 0.7 && q.SortBy == "occurrences" && q.SortDirection == "desc" &&
				q.Page == 1 && q.PageSize == 50),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(reportResult);

		// Act
		Results<Ok<ItemSimilarityResponse>, BadRequest<string>> result =
			await _controller.GetItemSimilarity(null, null, null, null, null, CancellationToken.None);

		// Assert
		Ok<ItemSimilarityResponse> okResult = Assert.IsType<Ok<ItemSimilarityResponse>>(result.Result);
		ItemSimilarityResponse response = okResult.Value!;
		response.TotalCount.Should().Be(1);
		response.Groups.Should().ContainSingle();
		response.Groups.First().CanonicalName.Should().Be("Milk");
		response.Groups.First().Occurrences.Should().Be(2);
		response.Groups.First().MaxSimilarity.Should().Be(0.85);
	}

	[Fact]
	public async Task GetItemSimilarity_PassesCustomParameters()
	{
		// Arrange
		AppReports.ItemSimilarityResult reportResult = new([], 0);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetItemSimilarityReportQuery>(q =>
				q.Threshold == 0.5 && q.SortBy == "canonicalName" && q.SortDirection == "asc" &&
				q.Page == 2 && q.PageSize == 25),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(reportResult);

		// Act
		Results<Ok<ItemSimilarityResponse>, BadRequest<string>> result =
			await _controller.GetItemSimilarity(0.5, "canonicalName", "asc", 2, 25, CancellationToken.None);

		// Assert
		Assert.IsType<Ok<ItemSimilarityResponse>>(result.Result);
		_mediatorMock.Verify(m => m.Send(
			It.Is<GetItemSimilarityReportQuery>(q =>
				q.Threshold == 0.5 && q.SortBy == "canonicalName" && q.SortDirection == "asc" &&
				q.Page == 2 && q.PageSize == 25),
			It.IsAny<CancellationToken>()), Times.Once);
	}

	[Theory]
	[InlineData(0.2)]
	[InlineData(0.96)]
	[InlineData(-1.0)]
	[InlineData(1.0)]
	public async Task GetItemSimilarity_ReturnsBadRequest_WhenThresholdOutOfRange(double threshold)
	{
		// Act
		Results<Ok<ItemSimilarityResponse>, BadRequest<string>> result =
			await _controller.GetItemSimilarity(threshold, null, null, null, null, CancellationToken.None);

		// Assert
		BadRequest<string> badResult = Assert.IsType<BadRequest<string>>(result.Result);
		badResult.Value.Should().Contain("threshold must be between 0.3 and 0.95");
	}

	[Fact]
	public async Task GetItemSimilarity_ReturnsBadRequest_WhenInvalidSortBy()
	{
		// Act
		Results<Ok<ItemSimilarityResponse>, BadRequest<string>> result =
			await _controller.GetItemSimilarity(null, "invalid", null, null, null, CancellationToken.None);

		// Assert
		BadRequest<string> badResult = Assert.IsType<BadRequest<string>>(result.Result);
		badResult.Value.Should().Contain("Invalid sortBy");
	}

	[Fact]
	public async Task GetItemSimilarity_ReturnsBadRequest_WhenInvalidSortDirection()
	{
		// Act
		Results<Ok<ItemSimilarityResponse>, BadRequest<string>> result =
			await _controller.GetItemSimilarity(null, null, "invalid", null, null, CancellationToken.None);

		// Assert
		BadRequest<string> badResult = Assert.IsType<BadRequest<string>>(result.Result);
		badResult.Value.Should().Contain("Invalid sortDirection");
	}

	[Fact]
	public async Task GetItemSimilarity_ReturnsBadRequest_WhenPageLessThanOne()
	{
		// Act
		Results<Ok<ItemSimilarityResponse>, BadRequest<string>> result =
			await _controller.GetItemSimilarity(null, null, null, 0, null, CancellationToken.None);

		// Assert
		BadRequest<string> badResult = Assert.IsType<BadRequest<string>>(result.Result);
		badResult.Value.Should().Be("page must be at least 1");
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	[InlineData(101)]
	public async Task GetItemSimilarity_ReturnsBadRequest_WhenPageSizeOutOfRange(int pageSize)
	{
		// Act
		Results<Ok<ItemSimilarityResponse>, BadRequest<string>> result =
			await _controller.GetItemSimilarity(null, null, null, null, pageSize, CancellationToken.None);

		// Assert
		BadRequest<string> badResult = Assert.IsType<BadRequest<string>>(result.Result);
		badResult.Value.Should().Be("pageSize must be between 1 and 100");
	}

	[Theory]
	[InlineData("canonicalName")]
	[InlineData("occurrences")]
	[InlineData("maxSimilarity")]
	public async Task GetItemSimilarity_AcceptsValidSortColumns(string sortBy)
	{
		// Arrange
		AppReports.ItemSimilarityResult reportResult = new([], 0);

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<GetItemSimilarityReportQuery>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(reportResult);

		// Act
		Results<Ok<ItemSimilarityResponse>, BadRequest<string>> result =
			await _controller.GetItemSimilarity(null, sortBy, null, null, null, CancellationToken.None);

		// Assert
		Assert.IsType<Ok<ItemSimilarityResponse>>(result.Result);
	}

	[Theory]
	[InlineData(0.3)]
	[InlineData(0.5)]
	[InlineData(0.7)]
	[InlineData(0.95)]
	public async Task GetItemSimilarity_AcceptsValidThresholds(double threshold)
	{
		// Arrange
		AppReports.ItemSimilarityResult reportResult = new([], 0);

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<GetItemSimilarityReportQuery>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(reportResult);

		// Act
		Results<Ok<ItemSimilarityResponse>, BadRequest<string>> result =
			await _controller.GetItemSimilarity(threshold, null, null, null, null, CancellationToken.None);

		// Assert
		Assert.IsType<Ok<ItemSimilarityResponse>>(result.Result);
	}

	[Fact]
	public async Task GetItemSimilarity_MapsResponseFieldsCorrectly()
	{
		// Arrange
		Guid itemId1 = Guid.NewGuid();
		Guid itemId2 = Guid.NewGuid();

		AppReports.ItemSimilarityResult reportResult = new(
		[
			new AppReports.ItemSimilarityGroup(
				"Coffee",
				["Coffee", "COFFEE", "Coffe"],
				[itemId1, itemId2],
				2,
				0.92),
		], 1);

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<GetItemSimilarityReportQuery>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(reportResult);

		// Act
		Results<Ok<ItemSimilarityResponse>, BadRequest<string>> result =
			await _controller.GetItemSimilarity(null, null, null, null, null, CancellationToken.None);

		// Assert
		Ok<ItemSimilarityResponse> okResult = Assert.IsType<Ok<ItemSimilarityResponse>>(result.Result);
		ItemSimilarityGroup group = okResult.Value!.Groups.First();
		group.CanonicalName.Should().Be("Coffee");
		group.Variants.Should().HaveCount(3);
		group.ItemIds.Should().HaveCount(2);
		group.Occurrences.Should().Be(2);
		group.MaxSimilarity.Should().Be(0.92);
	}

	[Fact]
	public async Task RenameItemSimilarityGroup_ReturnsOkResult()
	{
		// Arrange
		Guid id1 = Guid.NewGuid();
		Guid id2 = Guid.NewGuid();

		_mediatorMock.Setup(m => m.Send(
			It.Is<RenameItemSimilarityGroupCommand>(c =>
				c.ItemIds.Count == 2 && c.NewDescription == "Milk"),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(2);

		ItemSimilarityRenameRequest request = new()
		{
			ItemIds = [id1, id2],
			NewDescription = "Milk"
		};

		// Act
		Results<Ok<ItemSimilarityRenameResponse>, BadRequest<string>> result =
			await _controller.RenameItemSimilarityGroup(request, CancellationToken.None);

		// Assert
		Ok<ItemSimilarityRenameResponse> okResult = Assert.IsType<Ok<ItemSimilarityRenameResponse>>(result.Result);
		okResult.Value!.UpdatedCount.Should().Be(2);
	}

	[Fact]
	public async Task RenameItemSimilarityGroup_ReturnsBadRequest_WhenItemIdsEmpty()
	{
		// Arrange
		ItemSimilarityRenameRequest request = new()
		{
			ItemIds = [],
			NewDescription = "Milk"
		};

		// Act
		Results<Ok<ItemSimilarityRenameResponse>, BadRequest<string>> result =
			await _controller.RenameItemSimilarityGroup(request, CancellationToken.None);

		// Assert
		BadRequest<string> badResult = Assert.IsType<BadRequest<string>>(result.Result);
		badResult.Value.Should().Contain("itemIds must not be empty");
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public async Task RenameItemSimilarityGroup_ReturnsBadRequest_WhenDescriptionEmpty(string? description)
	{
		// Arrange
		ItemSimilarityRenameRequest request = new()
		{
			ItemIds = [Guid.NewGuid()],
			NewDescription = description!
		};

		// Act
		Results<Ok<ItemSimilarityRenameResponse>, BadRequest<string>> result =
			await _controller.RenameItemSimilarityGroup(request, CancellationToken.None);

		// Assert
		BadRequest<string> badResult = Assert.IsType<BadRequest<string>>(result.Result);
		badResult.Value.Should().Contain("newDescription must not be empty");
	}
}
