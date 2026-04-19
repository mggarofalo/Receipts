using System.Diagnostics;
using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class ItemSimilarityEdgeRefresher(
	IServiceScopeFactory scopeFactory,
	IDescriptionChangeSignal signal,
	ILogger<ItemSimilarityEdgeRefresher> logger) : BackgroundService
{
	// Matches the UI floor in ReportsController.GetItemSimilarity (threshold >= 0.3).
	// Edges below this would never be returned, so we don't store them.
	private const double MinThreshold = 0.3;

	// After this many consecutive failures, rethrow so .NET's default
	// BackgroundServiceExceptionBehavior.StopHost crashes the host. Docker restarts the container.
	private const int MaxConsecutiveFailures = 3;

	// Safety net: even if no signal arrives, refresh at least this often.
	private static readonly TimeSpan MaxIdleInterval = TimeSpan.FromHours(4);

	// Debounce window: after a signal, wait this long before refreshing so a burst of mutations
	// coalesces into a single refresh cycle.
	private static readonly TimeSpan DebounceQuietWindow = TimeSpan.FromSeconds(30);

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		int consecutiveFailures = 0;

		while (!stoppingToken.IsCancellationRequested)
		{
			// Wait for a dirty signal or the safety-timer deadline, whichever comes first.
			using (CancellationTokenSource waitCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken))
			{
				waitCts.CancelAfter(MaxIdleInterval);
				try
				{
					await signal.Reader.ReadAsync(waitCts.Token);
				}
				catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
				{
					return;
				}
				catch (OperationCanceledException)
				{
					// Safety timer fired; fall through to refresh.
				}
			}

			try
			{
				await Task.Delay(DebounceQuietWindow, stoppingToken);
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				return;
			}

			// Drain any additional dirty signals that arrived during the debounce window.
			while (signal.Reader.TryRead(out _))
			{
			}

			try
			{
				await RefreshAsync(stoppingToken);
				consecutiveFailures = 0;
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				return;
			}
			catch (Exception ex)
			{
				consecutiveFailures++;
				// Sentry.AspNetCore's ILogger integration forwards LogError calls to Sentry
				// automatically (see Program.cs UseSentry configuration). No explicit
				// SentrySdk.CaptureException needed here.
				logger.LogError(
					ex,
					"Item-similarity refresh failed (attempt {Attempt}/{Max})",
					consecutiveFailures,
					MaxConsecutiveFailures);

				if (consecutiveFailures >= MaxConsecutiveFailures)
				{
					// Rethrow so the host crashes (BackgroundServiceExceptionBehavior.StopHost
					// default). Docker restarts the container.
					throw;
				}
			}
		}
	}

	public async Task RefreshAsync(CancellationToken cancellationToken)
	{
		using IServiceScope scope = scopeFactory.CreateScope();
		IDbContextFactory<ApplicationDbContext> contextFactory =
			scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		// `%` operator and set_limit() require pg_trgm; skip silently on non-Postgres providers.
		if (!context.Database.IsNpgsql())
		{
			return;
		}

		Stopwatch sw = Stopwatch.StartNew();

		// One transaction:
		//   1. Set per-session pg_trgm threshold.
		//   2. INSERT/UPSERT edges where at least one side is unprocessed.
		//   3. Mark all unprocessed descriptions as processed.
		// MVCC keeps concurrent readers on the old snapshot until commit.
		const string refreshSql =
			"""
			SELECT set_limit({0});

			INSERT INTO "ItemSimilarityEdges" ("DescA", "DescB", "Score", "ComputedAt")
			SELECT a."Description", b."Description",
				   similarity(a."Description", b."Description"),
				   NOW()
			FROM "DistinctDescriptions" a
			JOIN "DistinctDescriptions" b
			  ON a."Description" < b."Description"
			 AND a."Description" % b."Description"
			WHERE a."ProcessedAt" IS NULL OR b."ProcessedAt" IS NULL
			ON CONFLICT ("DescA", "DescB") DO UPDATE
				SET "Score" = EXCLUDED."Score", "ComputedAt" = EXCLUDED."ComputedAt";

			UPDATE "DistinctDescriptions" SET "ProcessedAt" = NOW() WHERE "ProcessedAt" IS NULL;
			""";

		await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
		await context.Database.ExecuteSqlRawAsync(
			string.Format(System.Globalization.CultureInfo.InvariantCulture, refreshSql, MinThreshold),
			cancellationToken);
		await transaction.CommitAsync(cancellationToken);

		int edgeCount = await context.ItemSimilarityEdges.AsNoTracking().CountAsync(cancellationToken);
		logger.LogInformation(
			"Refreshed item-similarity edges: total={EdgeCount}, elapsed={ElapsedMs} ms",
			edgeCount,
			sw.ElapsedMilliseconds);
	}
}
