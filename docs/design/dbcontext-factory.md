# Design Document: Implementing a DbContextFactory Pattern for Dependency Injection

## Introduction

This document outlines the design for implementing a `DbContextFactory` pattern to facilitate dependency injection of `DbContext` instances in an ASP.NET Core application. The `DbContextFactory` pattern is particularly useful for scenarios where `DbContext` instances are needed outside of the typical request/response lifecycle, such as in background services or during design-time operations. This architecture also ensures that, as `DbContext` instances are registered as scoped services, they will be disposed of after each request and will not conflict with each other, avoiding the situation where one request's `DbContext` is used by another request and throws an exception.

## Objectives

- To provide a flexible and efficient way to create `DbContext` instances.
- To ensure that `DbContext` instances are properly configured and managed.
- To support both runtime and design-time scenarios.

## Architecture Overview

The architecture consists of the following components:

1. **Service Registration**: Configures the factory and `DbContext` in the dependency injection container.
2. **Repositories**: Utilize the `DbContextFactory` to perform CRUD operations.
3. **Design-Time Support**: Ensures that the factory can be used during design-time operations, such as migrations.

### 1. Service Registration

The factory and `DbContext` are registered in the dependency injection container. This ensures that they can be resolved and used throughout the application.

```csharp
public static class InfrastructureService
{
    public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextFactory<ApplicationDbContext>(options =>
        {
            string? connectionString = configuration["POSTGRES_CONNECTION_STRING"];
            options.UseNpgsql(connectionString, b =>
            {
                string? assemblyName = typeof(ApplicationDbContext).Assembly.FullName;
                b.MigrationsAssembly(assemblyName);
            });
        });

        // Register other services

        return services;
    }
}
```

### 2. Repositories

Repositories use the `DbContextFactory` to perform CRUD operations. For example, the `AccountRepository` uses the factory to create `DbContext` instances for database operations.

```csharp
public class AccountRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : IAccountRepository
{
	public async Task<AccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Accounts.FindAsync([id], cancellationToken);
	}

	// Other methods
}
```

### 3. Design-Time Support

To support design-time operations, such as migrations, a `DesignTimeDbContextFactory` is implemented. This factory uses environment variables or configuration files to set up the `DbContext`.

```csharp
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
	public ApplicationDbContext CreateDbContext(string[] args)
	{
		DbContextOptionsBuilder<ApplicationDbContext> builder = new();
		builder.UseNpgsql(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING"));
		return new ApplicationDbContext(builder.Options);
	}
}
```

## Testing

### DbContextHelpers

The `DbContextHelpers` class provides utility methods for creating in-memory `DbContext` instances for testing purposes, ensuring isolation and repeatability.

```csharp
public static ApplicationDbContext CreateInMemoryContext()
{
    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    optionsBuilder.UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}");

    var context = new ApplicationDbContext(optionsBuilder.Options);
    context.ResetDatabase();

    return context;
}
```

### TestDbContextFactory

The `TestDbContextFactory` class can be used to create `DbContext` instances in tests, ensuring isolation and repeatability.

## Conclusion

Implementing a `DbContextFactory` pattern provides a robust solution for managing `DbContext` instances in an ASP.NET Core application. It enhances flexibility, supports design-time operations, and integrates seamlessly with the dependency injection system. This design ensures that `DbContext` instances are configured consistently and can be used efficiently across different application scenarios.