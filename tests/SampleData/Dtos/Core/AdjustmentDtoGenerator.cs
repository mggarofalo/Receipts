using API.Generated.Dtos;

namespace SampleData.Dtos.Core;

public static class AdjustmentDtoGenerator
{
	public static CreateAdjustmentRequest GenerateCreateRequest()
	{
		return new CreateAdjustmentRequest
		{
			Type = "tip",
			Amount = 5.00,
			Description = null
		};
	}

	public static List<CreateAdjustmentRequest> GenerateCreateRequestList(int count)
	{
		return [.. Enumerable.Range(0, count).Select(_ => GenerateCreateRequest())];
	}

	public static UpdateAdjustmentRequest GenerateUpdateRequest()
	{
		return new UpdateAdjustmentRequest
		{
			Id = Guid.NewGuid(),
			Type = "tip",
			Amount = 5.00,
			Description = null
		};
	}

	public static List<UpdateAdjustmentRequest> GenerateUpdateRequestList(int count)
	{
		return [.. Enumerable.Range(0, count).Select(_ => GenerateUpdateRequest())];
	}
}
