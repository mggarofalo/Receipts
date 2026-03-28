using Application.Models;
using Domain.Core;

namespace Application.Interfaces.Services;

public interface ISubcategoryService : IService<Subcategory>
{
	Task<List<Subcategory>> CreateAsync(List<Subcategory> models, CancellationToken cancellationToken);
	Task UpdateAsync(List<Subcategory> models, CancellationToken cancellationToken);
	Task DeleteAsync(Guid id, CancellationToken cancellationToken);
	Task<PagedResult<Subcategory>> GetByCategoryIdAsync(Guid categoryId, int offset, int limit, SortParams sort, CancellationToken cancellationToken);
	Task<int> GetReceiptItemCountBySubcategoryNameAsync(string subcategoryName, CancellationToken cancellationToken);
}
