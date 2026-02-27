using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extensions;

public static class SoftDeleteQueryExtensions
{
	public static IQueryable<T> IncludeDeleted<T>(this IQueryable<T> query) where T : class, ISoftDeletable
	{
		return query.IgnoreQueryFilters();
	}

	public static IQueryable<T> OnlyDeleted<T>(this IQueryable<T> query) where T : class, ISoftDeletable
	{
		return query.IgnoreQueryFilters().Where(e => e.DeletedAt != null);
	}
}
