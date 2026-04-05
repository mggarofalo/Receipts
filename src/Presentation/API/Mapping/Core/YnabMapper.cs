using API.Generated.Dtos;
using Application.Models.Ynab;
using Riok.Mapperly.Abstractions;

namespace API.Mapping.Core;

[Mapper]
public partial class YnabMapper
{
	[MapperIgnoreTarget(nameof(YnabBudgetSummary.AdditionalProperties))]
	public partial YnabBudgetSummary ToBudgetSummary(YnabBudget source);

	[MapperIgnoreTarget(nameof(YnabBudgetListResponse.AdditionalProperties))]
	public YnabBudgetListResponse ToBudgetListResponse(List<YnabBudget> budgets)
	{
		return new YnabBudgetListResponse
		{
			Data = budgets.Select(ToBudgetSummary).ToList(),
		};
	}

	[MapperIgnoreTarget(nameof(YnabBudgetSettingsResponse.AdditionalProperties))]
	public YnabBudgetSettingsResponse ToBudgetSettingsResponse(YnabBudgetSelection source)
	{
		return new YnabBudgetSettingsResponse
		{
			SelectedBudgetId = source.SelectedBudgetId,
		};
	}

	[MapperIgnoreTarget(nameof(YnabSyncRecordResponse.AdditionalProperties))]
	public YnabSyncRecordResponse ToSyncRecordResponse(YnabSyncRecordDto source)
	{
		return new YnabSyncRecordResponse
		{
			Id = source.Id,
			LocalTransactionId = source.LocalTransactionId,
			YnabTransactionId = source.YnabTransactionId,
			YnabBudgetId = source.YnabBudgetId,
			YnabAccountId = source.YnabAccountId,
			SyncType = Enum.Parse<YnabSyncRecordResponseSyncType>(source.SyncType.ToString()),
			SyncStatus = Enum.Parse<YnabSyncRecordResponseSyncStatus>(source.SyncStatus.ToString()),
			SyncedAtUtc = source.SyncedAtUtc,
			LastError = source.LastError,
			CreatedAt = source.CreatedAt,
			UpdatedAt = source.UpdatedAt,
		};
	}
}
