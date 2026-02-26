using Domain.Core;

namespace Application.Interfaces.Services;

public interface ISubcategoryService : IService<Subcategory>
{
	Task<List<Subcategory>> CreateAsync(List<Subcategory> models, CancellationToken cancellationToken);
	Task UpdateAsync(List<Subcategory> models, CancellationToken cancellationToken);
	Task<List<Subcategory>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken);
}
