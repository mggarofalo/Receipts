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

	[MapperIgnoreTarget(nameof(YnabAccountSummary.AdditionalProperties))]
	public YnabAccountSummary ToAccountSummary(YnabAccount source)
	{
		return new YnabAccountSummary
		{
			Id = source.Id,
			Name = source.Name,
			Type = source.Type,
			OnBudget = source.OnBudget,
			Closed = source.Closed,
			Balance = source.Balance,
		};
	}

	[MapperIgnoreTarget(nameof(YnabAccountListResponse.AdditionalProperties))]
	public YnabAccountListResponse ToAccountListResponse(List<YnabAccount> accounts)
	{
		return new YnabAccountListResponse
		{
			Data = accounts.Select(ToAccountSummary).ToList(),
		};
	}

	[MapperIgnoreTarget(nameof(YnabAccountMappingResponse.AdditionalProperties))]
	public YnabAccountMappingResponse ToAccountMappingResponse(YnabAccountMappingDto source)
	{
		return new YnabAccountMappingResponse
		{
			Id = source.Id,
			ReceiptsAccountId = source.ReceiptsAccountId,
			YnabAccountId = source.YnabAccountId,
			YnabAccountName = source.YnabAccountName,
			YnabBudgetId = source.YnabBudgetId,
			CreatedAt = source.CreatedAt,
			UpdatedAt = source.UpdatedAt,
		};
	}

	[MapperIgnoreTarget(nameof(YnabAccountMappingListResponse.AdditionalProperties))]
	public YnabAccountMappingListResponse ToAccountMappingListResponse(List<YnabAccountMappingDto> mappings)
	{
		return new YnabAccountMappingListResponse
		{
			Data = mappings.Select(ToAccountMappingResponse).ToList(),
		};
	}
}
