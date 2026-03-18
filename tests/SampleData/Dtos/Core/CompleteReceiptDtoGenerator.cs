using API.Generated.Dtos;

namespace SampleData.Dtos.Core;

public static class CompleteReceiptDtoGenerator
{
	public static CreateCompleteReceiptRequest GenerateCreateRequest(
		int transactionCount = 1,
		int itemCount = 1)
	{
		return new CreateCompleteReceiptRequest
		{
			Receipt = ReceiptDtoGenerator.GenerateCreateRequest(),
			Transactions = TransactionDtoGenerator.GenerateCreateRequestList(transactionCount),
			Items = ReceiptItemDtoGenerator.GenerateCreateRequestList(itemCount),
		};
	}
}
