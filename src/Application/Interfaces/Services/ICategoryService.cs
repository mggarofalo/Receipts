using Domain.Core;

namespace Application.Interfaces.Services;

public interface ICategoryService : IService<Category>
{
	Task<List<Category>> CreateAsync(List<Category> models, CancellationToken cancellationToken);
	Task UpdateAsync(List<Category> models, CancellationToken cancellationToken);
}
