using Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class AuthAuditCleanupService(
	IServiceScopeFactory scopeFactory,
	ILogger<AuthAuditCleanupService> logger) : BackgroundService
{
	private const int RetentionDays = 180;
	private static readonly TimeSpan Interval = TimeSpan.FromDays(1);

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				using IServiceScope scope = scopeFactory.CreateScope();
				IAuthAuditService auditService = scope.ServiceProvider.GetRequiredService<IAuthAuditService>();

				int deleted = await auditService.CleanupOldEntriesAsync(RetentionDays, stoppingToken);
				logger.LogInformation("Auth audit cleanup completed: deleted {Count} entries older than {RetentionDays} days", deleted, RetentionDays);
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				break;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error during auth audit cleanup");
			}

			await Task.Delay(Interval, stoppingToken);
		}
	}
}
