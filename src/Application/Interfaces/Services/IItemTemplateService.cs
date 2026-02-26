using Domain.Core;

namespace Application.Interfaces.Services;

public interface IItemTemplateService : IService<ItemTemplate>
{
	Task<List<ItemTemplate>> CreateAsync(List<ItemTemplate> models, CancellationToken cancellationToken);
	Task UpdateAsync(List<ItemTemplate> models, CancellationToken cancellationToken);
}
