namespace Application.Models;

public record BackupImportResult(
	int AccountsCreated,
	int AccountsUpdated,
	int CardsCreated,
	int CardsUpdated,
	int CategoriesCreated,
	int CategoriesUpdated,
	int SubcategoriesCreated,
	int SubcategoriesUpdated,
	int ItemTemplatesCreated,
	int ItemTemplatesUpdated,
	int ReceiptsCreated,
	int ReceiptsUpdated,
	int ReceiptItemsCreated,
	int ReceiptItemsUpdated,
	int TransactionsCreated,
	int TransactionsUpdated,
	int AdjustmentsCreated,
	int AdjustmentsUpdated)
{
	public int TotalCreated => AccountsCreated + CardsCreated + CategoriesCreated + SubcategoriesCreated +
		ItemTemplatesCreated + ReceiptsCreated + ReceiptItemsCreated +
		TransactionsCreated + AdjustmentsCreated;

	public int TotalUpdated => AccountsUpdated + CardsUpdated + CategoriesUpdated + SubcategoriesUpdated +
		ItemTemplatesUpdated + ReceiptsUpdated + ReceiptItemsUpdated +
		TransactionsUpdated + AdjustmentsUpdated;
}
