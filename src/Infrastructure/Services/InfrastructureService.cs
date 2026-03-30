using Application.Interfaces;
using Application.Interfaces.Services;
using Common;
using Infrastructure.Entities;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Mapping;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure.Services;

public static class InfrastructureService
{
	public static bool IsDatabaseConfigured(IConfiguration configuration)
	{
		// Aspire-injected connection string takes precedence
		if (!string.IsNullOrEmpty(configuration[$"ConnectionStrings:{ConfigurationVariables.AspireConnectionStringName}"]))
		{
			return true;
		}

		// Fall back to individual POSTGRES_* environment variables (non-Aspire deployments)
		return !string.IsNullOrEmpty(configuration[ConfigurationVariables.PostgresHost])
			&& !string.IsNullOrEmpty(configuration[ConfigurationVariables.PostgresPort])
			&& !string.IsNullOrEmpty(configuration[ConfigurationVariables.PostgresUser])
			&& !string.IsNullOrEmpty(configuration[ConfigurationVariables.PostgresPassword])
			&& !string.IsNullOrEmpty(configuration[ConfigurationVariables.PostgresDb]);
	}

	public static string GetConnectionString(IConfiguration configuration)
	{
		// Aspire-injected connection string (set by WithReference(db) in AppHost)
		string? aspireConnectionString = configuration[$"ConnectionStrings:{ConfigurationVariables.AspireConnectionStringName}"];
		if (!string.IsNullOrEmpty(aspireConnectionString))
		{
			return aspireConnectionString;
		}

		// Build from individual POSTGRES_* environment variables
		Npgsql.NpgsqlConnectionStringBuilder builder = new()
		{
			Host = configuration[ConfigurationVariables.PostgresHost]!,
			Port = int.Parse(configuration[ConfigurationVariables.PostgresPort]!),
			Username = configuration[ConfigurationVariables.PostgresUser]!,
			Password = configuration[ConfigurationVariables.PostgresPassword]!,
			Database = configuration[ConfigurationVariables.PostgresDb]!
		};

		return builder.ConnectionString;
	}

	public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
	{
		if (IsDatabaseConfigured(configuration))
		{
			services.AddSingleton<NpgsqlDataSource>(sp =>
			{
				NpgsqlDataSourceBuilder dataSourceBuilder = new(GetConnectionString(configuration));
				dataSourceBuilder.UseVector();
				return dataSourceBuilder.Build();
			});

			services.AddDbContextFactory<ApplicationDbContext>((sp, options) =>
			{
				NpgsqlDataSource dataSource = sp.GetRequiredService<NpgsqlDataSource>();
				options.UseNpgsql(dataSource, b =>
				{
					string? assemblyName = typeof(ApplicationDbContext).Assembly.FullName;
					b.MigrationsAssembly(assemblyName);
					b.UseVector();
				});
				options.ConfigureWarnings(w => w.Log(
					(RelationalEventId.PendingModelChangesWarning, LogLevel.Warning)));
			});
		}
		else
		{
			services.AddDbContextFactory<ApplicationDbContext>(options =>
			{
				options.UseNpgsql();
				options.ConfigureWarnings(w => w.Log(
					(RelationalEventId.PendingModelChangesWarning, LogLevel.Warning)));
			});
		}

		// Fallback ICurrentUserAccessor for when no HTTP context is available (tests, background services).
		// The API layer registers the real implementation before this, so TryAdd is a no-op in production.
		services.TryAddScoped<ICurrentUserAccessor, NullCurrentUserAccessor>();

		services
			.AddIdentityCore<ApplicationUser>()
			.AddRoles<IdentityRole>()
			.AddEntityFrameworkStores<ApplicationDbContext>();

		// Override the factory's scoped ApplicationDbContext registration to use 2-param constructor.
		// AddDbContextFactory auto-registers a scoped context that delegates to the singleton factory,
		// which uses root provider and can't resolve scoped ICurrentUserAccessor.
		// AddEntityFrameworkStores also re-registers the scoped context, so this MUST come after it.
		services.AddScoped(sp =>
		{
			DbContextOptions<ApplicationDbContext> options =
				sp.GetRequiredService<DbContextOptions<ApplicationDbContext>>();
			ICurrentUserAccessor accessor = sp.GetRequiredService<ICurrentUserAccessor>();
			return new ApplicationDbContext(options, accessor);
		});

		services
			.AddScoped<IReceiptService, ReceiptService>()
			.AddScoped<IAccountService, AccountService>()
			.AddScoped<ICategoryService, CategoryService>()
			.AddScoped<ISubcategoryService, SubcategoryService>()
			.AddScoped<ITransactionService, TransactionService>()
			.AddScoped<IAdjustmentService, AdjustmentService>()
			.AddScoped<IReceiptItemService, ReceiptItemService>()
			.AddScoped<ICompleteReceiptService, CompleteReceiptService>()
			.AddScoped<IItemTemplateService, ItemTemplateService>()
			.AddScoped<IItemTemplateSimilarityService, ItemTemplateSimilarityService>()
			.AddScoped<IReceiptRepository, ReceiptRepository>()
			.AddScoped<IAccountRepository, AccountRepository>()
			.AddScoped<ICategoryRepository, CategoryRepository>()
			.AddScoped<ISubcategoryRepository, SubcategoryRepository>()
			.AddScoped<ITransactionRepository, TransactionRepository>()
			.AddScoped<IAdjustmentRepository, AdjustmentRepository>()
			.AddScoped<IReceiptItemRepository, ReceiptItemRepository>()
			.AddScoped<IItemTemplateRepository, ItemTemplateRepository>()
			.AddScoped<IDatabaseMigratorService, DatabaseMigratorService>()
			.AddScoped<ITokenService, TokenService>()
			.AddScoped<IApiKeyService, ApiKeyService>()
			.AddScoped<IAuditService, AuditService>()
			.AddScoped<IAuthAuditService, AuthAuditService>()
			.AddScoped<IUserService, UserService>()
			.AddScoped<ITrashService, TrashService>()
			.AddScoped<IDashboardService, DashboardService>()
			.AddScoped<IReportService, ReportService>()
			.AddScoped<IImageStorageService, LocalImageStorageService>()
			.AddScoped<IImageProcessingService, ImageProcessingService>();

		// Singleton AI/ML services (local models — always available)
		services.AddSingleton<IEmbeddingService, OnnxEmbeddingService>();
		services.AddSingleton<IOcrEngine, TesseractOcrEngine>();
		services.AddHostedService<EmbeddingGenerationService>();

		services.AddHostedService<AuthAuditCleanupService>();

		services
			.AddSingleton<AccountMapper>()
			.AddSingleton<CategoryMapper>()
			.AddSingleton<SubcategoryMapper>()
			.AddSingleton<ReceiptMapper>()
			.AddSingleton<ReceiptItemMapper>()
			.AddSingleton<TransactionMapper>()
			.AddSingleton<AdjustmentMapper>()
			.AddSingleton<ItemTemplateMapper>();

		return services;
	}
}