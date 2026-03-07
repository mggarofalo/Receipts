using System.Linq.Expressions;
using Application.Models;

namespace Infrastructure.Extensions;

public static class QueryableSortExtensions
{
	public static IOrderedQueryable<T> ApplySort<T>(
		this IQueryable<T> query,
		SortParams sort,
		Dictionary<string, Expression<Func<T, object>>> allowedColumns,
		Expression<Func<T, object>> defaultSort,
		bool defaultDescending = false)
	{
		Expression<Func<T, object>> sortExpression = defaultSort;
		bool descending = defaultDescending;

		if (!string.IsNullOrWhiteSpace(sort.SortBy)
			&& allowedColumns.TryGetValue(sort.SortBy, out Expression<Func<T, object>>? column))
		{
			sortExpression = column;
			descending = sort.IsDescending;
		}

		return descending
			? query.OrderByDescending(sortExpression)
			: query.OrderBy(sortExpression);
	}
}
