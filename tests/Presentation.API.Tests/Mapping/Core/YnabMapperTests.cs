using API.Generated.Dtos;
using API.Mapping.Core;
using Application.Models.Ynab;

namespace Presentation.API.Tests.Mapping.Core;

public class YnabMapperTests
{
	private readonly YnabMapper _mapper = new();

	[Fact]
	public void ToBudgetListResponse_MapsAllBudgets()
	{
		// Arrange
		List<YnabBudget> budgets =
		[
			new("budget-1", "My Budget"),
			new("budget-2", "Other Budget"),
		];

		// Act
		YnabBudgetListResponse result = _mapper.ToBudgetListResponse(budgets);

		// Assert
		Assert.Equal(2, result.Data.Count);

		YnabBudgetSummary first = result.Data.ElementAt(0);
		Assert.Equal("budget-1", first.Id);
		Assert.Equal("My Budget", first.Name);

		YnabBudgetSummary second = result.Data.ElementAt(1);
		Assert.Equal("budget-2", second.Id);
		Assert.Equal("Other Budget", second.Name);
	}

	[Fact]
	public void ToBudgetListResponse_MapsEmptyList()
	{
		// Arrange
		List<YnabBudget> budgets = [];

		// Act
		YnabBudgetListResponse result = _mapper.ToBudgetListResponse(budgets);

		// Assert
		Assert.Empty(result.Data);
	}

	[Fact]
	public void ToBudgetSettingsResponse_MapsSelectedBudgetId()
	{
		// Arrange
		string budgetId = Guid.NewGuid().ToString();
		YnabBudgetSelection selection = new(budgetId);

		// Act
		YnabBudgetSettingsResponse result = _mapper.ToBudgetSettingsResponse(selection);

		// Assert
		Assert.Equal(budgetId, result.SelectedBudgetId);
	}

	[Fact]
	public void ToBudgetSettingsResponse_MapsNullBudgetId()
	{
		// Arrange
		YnabBudgetSelection selection = new(null);

		// Act
		YnabBudgetSettingsResponse result = _mapper.ToBudgetSettingsResponse(selection);

		// Assert
		Assert.Null(result.SelectedBudgetId);
	}
}
