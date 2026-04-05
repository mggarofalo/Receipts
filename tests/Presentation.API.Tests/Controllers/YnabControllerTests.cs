using API.Controllers;
using API.Generated.Dtos;
using API.Mapping.Core;
using Application.Commands.Ynab.SelectBudget;
using Application.Interfaces.Services;
using Application.Models.Ynab;
using Application.Queries.Core.Ynab;
using FluentAssertions;
using Infrastructure.Ynab;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Presentation.API.Tests.Controllers;

public class YnabControllerTests
{
	private readonly Mock<IMediator> _mediatorMock;
	private readonly Mock<IYnabApiClient> _ynabClientMock;
	private readonly YnabMapper _mapper;
	private readonly Mock<ILogger<YnabController>> _loggerMock;
	private readonly YnabController _controller;

	public YnabControllerTests()
	{
		_mediatorMock = new Mock<IMediator>();
		_ynabClientMock = new Mock<IYnabApiClient>();
		_mapper = new YnabMapper();
		_loggerMock = ControllerTestHelpers.GetLoggerMock<YnabController>();
		_controller = new YnabController(_mediatorMock.Object, _ynabClientMock.Object, _mapper, _loggerMock.Object);
		_controller.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext()
		};
	}

	[Fact]
	public async Task GetBudgets_Returns200_WithBudgetList_WhenConfigured()
	{
		// Arrange
		_ynabClientMock.Setup(c => c.IsConfigured).Returns(true);

		List<YnabBudget> budgets =
		[
			new("budget-1", "My Budget"),
			new("budget-2", "Other Budget"),
		];

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<GetYnabBudgetsQuery>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(budgets);

		// Act
		Results<Ok<YnabBudgetListResponse>, StatusCodeHttpResult> result = await _controller.GetBudgets(CancellationToken.None);

		// Assert
		Ok<YnabBudgetListResponse> okResult = Assert.IsType<Ok<YnabBudgetListResponse>>(result.Result);
		YnabBudgetListResponse response = okResult.Value!;
		response.Data.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetBudgets_Returns503_WhenNotConfigured()
	{
		// Arrange
		_ynabClientMock.Setup(c => c.IsConfigured).Returns(false);

		// Act
		Results<Ok<YnabBudgetListResponse>, StatusCodeHttpResult> result = await _controller.GetBudgets(CancellationToken.None);

		// Assert
		StatusCodeHttpResult statusResult = Assert.IsType<StatusCodeHttpResult>(result.Result);
		statusResult.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
	}

	[Fact]
	public async Task GetBudgets_Returns503_OnYnabAuthException()
	{
		// Arrange
		_ynabClientMock.Setup(c => c.IsConfigured).Returns(true);

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<GetYnabBudgetsQuery>(),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new YnabAuthException("Invalid token"));

		// Act
		Results<Ok<YnabBudgetListResponse>, StatusCodeHttpResult> result = await _controller.GetBudgets(CancellationToken.None);

		// Assert
		StatusCodeHttpResult statusResult = Assert.IsType<StatusCodeHttpResult>(result.Result);
		statusResult.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
	}

	[Fact]
	public async Task GetBudgetSettings_Returns200_WithSelection()
	{
		// Arrange
		string budgetId = Guid.NewGuid().ToString();
		_mediatorMock.Setup(m => m.Send(
			It.IsAny<GetSelectedYnabBudgetQuery>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(new YnabBudgetSelection(budgetId));

		// Act
		Ok<YnabBudgetSettingsResponse> result = await _controller.GetBudgetSettings(CancellationToken.None);

		// Assert
		YnabBudgetSettingsResponse response = result.Value!;
		response.SelectedBudgetId.Should().Be(budgetId);
	}

	[Fact]
	public async Task SelectBudget_Returns204()
	{
		// Arrange
		string budgetId = Guid.NewGuid().ToString();
		_mediatorMock.Setup(m => m.Send(
			It.Is<SelectYnabBudgetCommand>(c => c.BudgetId == budgetId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(Unit.Value);

		SelectYnabBudgetRequest request = new() { BudgetId = budgetId };

		// Act
		NoContent result = await _controller.SelectBudget(request, CancellationToken.None);

		// Assert
		Assert.IsType<NoContent>(result);
		_mediatorMock.Verify(m => m.Send(
			It.Is<SelectYnabBudgetCommand>(c => c.BudgetId == budgetId),
			It.IsAny<CancellationToken>()), Times.Once);
	}
}
