namespace Application.Utilities;

public static class YnabImportId
{
	private const int MaxLength = 36;
	private const int ReceiptPrefixLength = 6;

	public static string Generate(long milliunits, DateOnly date, Guid receiptId, int occurrence)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(occurrence, 1);

		string receiptPrefix = receiptId.ToString("N")[..ReceiptPrefixLength];
		string importId = $"YNAB:{milliunits}:{date:yyyy-MM-dd}:{receiptPrefix}:{occurrence}";

		if (importId.Length > MaxLength)
		{
			throw new InvalidOperationException(
				$"Generated import_id '{importId}' exceeds YNAB's {MaxLength}-character limit (length: {importId.Length}).");
		}

		return importId;
	}
}
