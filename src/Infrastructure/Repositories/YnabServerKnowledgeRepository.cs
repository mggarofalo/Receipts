using Infrastructure.Entities.Core;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class YnabServerKnowledgeRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : IYnabServerKnowledgeRepository
{
	public async Task<long?> GetAsync(string budgetId, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		YnabServerKnowledgeEntity? entity = await context.YnabServerKnowledge.FindAsync([budgetId], cancellationToken);
		return entity?.ServerKnowledge;
	}

	public async Task UpsertAsync(string budgetId, long serverKnowledge, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();

		int updated = await context.YnabServerKnowledge
			.Where(e => e.BudgetId == budgetId)
			.ExecuteUpdateAsync(s => s
				.SetProperty(e => e.ServerKnowledge, serverKnowledge)
				.SetProperty(e => e.UpdatedAt, DateTimeOffset.UtcNow),
				cancellationToken);

		if (updated == 0)
		{
			context.YnabServerKnowledge.Add(new YnabServerKnowledgeEntity
			{
				BudgetId = budgetId,
				ServerKnowledge = serverKnowledge,
				UpdatedAt = DateTimeOffset.UtcNow,
			});

			try
			{
				await context.SaveChangesAsync(cancellationToken);
			}
			catch (DbUpdateException)
			{
				// Lost race — another thread inserted first. Retry as update.
				using ApplicationDbContext retryContext = contextFactory.CreateDbContext();
				await retryContext.YnabServerKnowledge
					.Where(e => e.BudgetId == budgetId)
					.ExecuteUpdateAsync(s => s
						.SetProperty(e => e.ServerKnowledge, serverKnowledge)
						.SetProperty(e => e.UpdatedAt, DateTimeOffset.UtcNow),
						cancellationToken);
			}
		}
	}
}
