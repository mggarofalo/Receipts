using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;
using Infrastructure.Mapping;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using SampleData.Domain.Core;
using SampleData.Entities;

namespace Infrastructure.Tests.Repositories;

public class AccountRepositoryTests
{
	private readonly IMapper _mapper;

	public AccountRepositoryTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<AccountMappingProfile>();
		});

		configuration.AssertConfigurationIsValid();

		_mapper = configuration.CreateMapper();
	}

	private static ApplicationDbContext GetContext()
	{
		DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("TestDatabase")
			.Options;

		ApplicationDbContext context = new(options);
		context.Database.EnsureDeleted();
		context.Database.EnsureCreated();

		return context;
	}

	[Fact]
	public async Task GetByIdAsync_ExistingId_ReturnsAccount()
	{
		// Arrange
		AccountEntity entity = AccountEntityGenerator.Generate();
		Account expected = _mapper.Map<Account>(entity);

		using ApplicationDbContext context = GetContext();
		await context.Accounts.AddAsync(entity);
		await context.SaveChangesAsync();

		AccountRepository repository = new(context, _mapper);

		// Act
		Account? actual = await repository.GetByIdAsync(entity.Id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public async Task GetByIdAsync_NonExistingId_ReturnsNull()
	{
		// Arrange
		using ApplicationDbContext context = GetContext();
		AccountRepository repository = new(context, _mapper);

		// Act
		Account? result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public async Task GetByTransactionIdAsync_ExistingTransactionId_ReturnsCorrectAccount()
	{
		// Arrange
		AccountEntity accountEntity = AccountEntityGenerator.Generate();
		Account expected = _mapper.Map<Account>(accountEntity);
		TransactionEntity transactionEntity = TransactionEntityGenerator.Generate();
		transactionEntity.AccountId = accountEntity.Id;

		using ApplicationDbContext context = GetContext();
		await context.Accounts.AddAsync(accountEntity);
		await context.Transactions.AddAsync(transactionEntity);
		await context.SaveChangesAsync();

		AccountRepository repository = new(context, _mapper);

		// Act
		Account? actual = await repository.GetByTransactionIdAsync(transactionEntity.Id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public async Task GetByTransactionIdAsync_NonExistingTransactionId_ReturnsNull()
	{
		// Arrange
		using ApplicationDbContext context = GetContext();
		AccountRepository repository = new(context, _mapper);

		// Act
		Account? result = await repository.GetByTransactionIdAsync(Guid.NewGuid(), CancellationToken.None);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public async Task GetAllAsync_ReturnsAllAccounts()
	{
		// Arrange
		List<AccountEntity> entities = AccountEntityGenerator.GenerateList(2);
		List<Account> expected = entities.Select(_mapper.Map<Account>).ToList();

		using ApplicationDbContext context = GetContext();
		await context.Accounts.AddRangeAsync(entities);
		await context.SaveChangesAsync();

		AccountRepository repository = new(context, _mapper);

		// Act
		List<Account> actual = await repository.GetAllAsync(CancellationToken.None);

		// Assert
		Assert.Equal(entities.Count, actual.Count);
		Assert.All(actual, r => Assert.Contains(r, expected));
		Assert.All(expected, a => Assert.Contains(a, actual));
	}

	[Fact]
	public async Task CreateAsync_ValidAccounts_ReturnsCreatedAccounts()
	{
		// Arrange
		List<Account> expected = AccountGenerator.GenerateList(2);
		List<AccountEntity> accountEntities = expected.Select(_mapper.Map<AccountEntity>).ToList();

		using ApplicationDbContext context = GetContext();
		AccountRepository repository = new(context, _mapper);

		// Act
		List<Account> actual = await repository.CreateAsync(expected, CancellationToken.None);

		// Assert
		Assert.Equal(expected.Count, actual.Count);
		Assert.Equal(expected.Select(a => a.Id), actual.Select(r => r.Id));
	}

	[Fact]
	public async Task UpdateAsync_ValidAccount_UpdatesAccount()
	{
		// Arrange
		AccountEntity entity = AccountEntityGenerator.Generate();
		Account expected = _mapper.Map<Account>(entity);

		using ApplicationDbContext context = GetContext();
		await context.Accounts.AddAsync(entity);
		await context.SaveChangesAsync();

		AccountRepository repository = new(context, _mapper);

		// Modify account
		expected.Name = "Updated " + expected.Name;
		expected.IsActive = !expected.IsActive;

		// Act
		await repository.UpdateAsync([expected], CancellationToken.None);
		await repository.SaveChangesAsync(CancellationToken.None);
		AccountEntity updatedEntity = await context.Accounts.FirstAsync(e => e.Id == expected.Id);

		// Assert
		Assert.Equal(expected.Name, updatedEntity.Name);
		Assert.Equal(expected.IsActive, updatedEntity.IsActive);
	}

	[Fact]
	public async Task DeleteAsync_ValidIds_DeletesAccounts()
	{
		// Arrange
		List<AccountEntity> entities = AccountEntityGenerator.GenerateList(3);
		List<Guid> idsToDelete = entities.Take(2).Select(e => e.Id).ToList();

		using ApplicationDbContext context = GetContext();
		await context.Accounts.AddRangeAsync(entities);
		await context.SaveChangesAsync();

		AccountRepository repository = new(context, _mapper);

		// Act
		await repository.DeleteAsync(idsToDelete, CancellationToken.None);
		await repository.SaveChangesAsync(CancellationToken.None);
		List<AccountEntity> remainingEntities = await context.Accounts.ToListAsync();

		// Assert
		Assert.Single(remainingEntities);
		Assert.DoesNotContain(remainingEntities, e => idsToDelete.Contains(e.Id));
	}

	[Fact]
	public async Task ExistsAsync_ExistingId_ReturnsTrue()
	{
		// Arrange
		AccountEntity entity = AccountEntityGenerator.Generate();

		using ApplicationDbContext context = GetContext();
		await context.Accounts.AddAsync(entity);
		await context.SaveChangesAsync();

		AccountRepository repository = new(context, _mapper);

		// Act
		bool result = await repository.ExistsAsync(entity.Id, CancellationToken.None);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public async Task ExistsAsync_NonExistingId_ReturnsFalse()
	{
		// Arrange
		using ApplicationDbContext context = GetContext();
		AccountRepository repository = new(context, _mapper);

		// Act
		bool result = await repository.ExistsAsync(Guid.NewGuid(), CancellationToken.None);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public async Task GetCountAsync_ReturnsCorrectCount()
	{
		// Arrange
		List<AccountEntity> entities = AccountEntityGenerator.GenerateList(3);

		using ApplicationDbContext context = GetContext();
		await context.Accounts.AddRangeAsync(entities);
		await context.SaveChangesAsync();

		AccountRepository repository = new(context, _mapper);

		// Act
		int count = await repository.GetCountAsync(CancellationToken.None);

		// Assert
		Assert.Equal(entities.Count, count);
	}
}