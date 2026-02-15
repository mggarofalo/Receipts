using API.Generated.Dtos;

namespace SampleData.Dtos.Core;

public static class ReceiptItemDtoGenerator
{
	public static CreateReceiptItemRequest GenerateCreateRequest()
	{
		return new CreateReceiptItemRequest
		{
			ReceiptItemCode = "ITEM-001",
			Description = "Test Item",
			Quantity = 2.0,
			UnitPrice = 9.99,
			Category = "Test Category",
			Subcategory = "Test Subcategory"
		};
	}

	public static List<CreateReceiptItemRequest> GenerateCreateRequestList(int count)
	{
		return [.. Enumerable.Range(0, count).Select(_ => GenerateCreateRequest())];
	}

	public static UpdateReceiptItemRequest GenerateUpdateRequest()
	{
		return new UpdateReceiptItemRequest
		{
			Id = Guid.NewGuid(),
			ReceiptItemCode = "ITEM-001",
			Description = "Test Item",
			Quantity = 2.0,
			UnitPrice = 9.99,
			Category = "Test Category",
			Subcategory = "Test Subcategory"
		};
	}

	public static List<UpdateReceiptItemRequest> GenerateUpdateRequestList(int count)
	{
		return [.. Enumerable.Range(0, count).Select(_ => GenerateUpdateRequest())];
	}
}
