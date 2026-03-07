namespace Application.Models;

public static class SortableColumns
{
	public static readonly IReadOnlySet<string> Account = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "accountCode", "name", "isActive" };
	public static readonly IReadOnlySet<string> Receipt = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "location", "date", "taxAmount" };
	public static readonly IReadOnlySet<string> Transaction = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "amount", "date" };
	public static readonly IReadOnlySet<string> ReceiptItem = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "description", "quantity", "unitPrice", "totalAmount", "category" };
	public static readonly IReadOnlySet<string> Category = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "name" };
	public static readonly IReadOnlySet<string> Subcategory = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "name" };
	public static readonly IReadOnlySet<string> ItemTemplate = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "name" };
	public static readonly IReadOnlySet<string> Adjustment = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "type", "amount", "description" };
	public static readonly IReadOnlySet<string> User = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "email", "firstName", "lastName", "createdAt" };

	private static readonly HashSet<string> ValidDirections = new(StringComparer.OrdinalIgnoreCase) { "asc", "desc" };

	public static bool IsValidDirection(string? direction) =>
		direction is null || ValidDirections.Contains(direction);
}
