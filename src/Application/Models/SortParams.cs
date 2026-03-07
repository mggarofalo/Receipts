namespace Application.Models;

public record SortParams(string? SortBy, string? SortDirection)
{
	public static SortParams Default => new(null, null);
	public bool IsDescending => string.Equals(SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
}
