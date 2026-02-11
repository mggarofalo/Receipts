using AutoMapper;
using Domain.Core;
using FluentAssertions;
using Infrastructure.Entities.Core;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Services;
using Moq;
using SampleData.Domain.Core;
using SampleData.Entities;

namespace Infrastructure.Tests.Services;

public class AccountServiceTests
{
	private readonly Mock<IAccountRepository> _mockRepository;
	private readonly Mock<IMapper> _mockMapper;
	private readonly AccountService _service;

	public AccountServiceTests()
	{
		_mockRepository = new Mock<IAccountRepository>();
		_mockMapper = new Mock<IMapper>();
		_service = new AccountService(_mockRepository.Object, _mockMapper.Object);
	}

	[Fact]
	public async Task CreateAsync_ValidAccounts_CallsRepositoryCreateAsyncAndReturnsCreatedAccounts()
	{
		// Arrange
		List<Account> expected = AccountGenerator.GenerateList(2);
		List<AccountEntity> entities = AccountEntityGenerator.GenerateList(2);
		List<AccountEntity> createdEntities = AccountEntityGenerator.GenerateList(2);

		_mockMapper.Setup(m => m.Map<AccountEntity>(It.IsAny<Account>())).Returns<Account>(a => entities.First());
		_mockRepository.Setup(r => r.CreateAsync(It.IsAny<List<AccountEntity>>(), It.IsAny<CancellationToken>())).ReturnsAsync(createdEntities);
		_mockMapper.Setup(m => m.Map<Account>(It.IsAny<AccountEntity>())).Returns<AccountEntity>(e => expected.First());

		// Act
		List<Account> actual = await _service.CreateAsync(expected, CancellationToken.None);

		// Assert
		Assert.Equal(expected.Count, actual.Count);
		_mockRepository.Verify(r => r.CreateAsync(It.IsAny<List<AccountEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task DeleteAsync_ValidIds_CallsRepositoryDeleteAsync()
	{
		// Arrange
		List<Guid> ids = [Guid.NewGuid(), Guid.NewGuid()];

		// Act
		await _service.DeleteAsync(ids, CancellationToken.None);

		// Assert
		_mockRepository.Verify(r => r.DeleteAsync(ids, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task ExistsAsync_ValidId_ReturnsExpectedResult()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		bool expected = true;
		_mockRepository.Setup(r => r.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		// Act
		bool actual = await _service.ExistsAsync(id, CancellationToken.None);

		// Assert
		Assert.Equal(expected, actual);
	}

	[Fact]
	public async Task GetAllAsync_ReturnsAllAccounts()
	{
		// Arrange
		List<AccountEntity> entities = AccountEntityGenerator.GenerateList(3);
		List<Account> expected = AccountGenerator.GenerateList(3);

		_mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entities);
		_mockMapper.Setup(m => m.Map<Account>(It.IsAny<AccountEntity>())).Returns<AccountEntity>(e => expected.First());

		// Act
		List<Account> actual = await _service.GetAllAsync(CancellationToken.None);

		// Assert
		Assert.Equal(expected.Count, actual.Count);
	}

	[Fact]
	public async Task GetByIdAsync_ExistingId_ReturnsAccount()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		AccountEntity entity = AccountEntityGenerator.Generate();
		Account expected = AccountGenerator.Generate();

		_mockRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
		_mockMapper.Setup(m => m.Map<Account>(entity)).Returns(expected);

		// Act
		Account? actual = await _service.GetByIdAsync(id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		actual.Should().BeSameAs(expected);
	}

	[Fact]
	public async Task GetByIdAsync_NonExistingId_ReturnsNull()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		_mockRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((AccountEntity?)null);

		// Act
		Account? actual = await _service.GetByIdAsync(id, CancellationToken.None);

		// Assert
		Assert.Null(actual);
	}

	[Fact]
	public async Task GetByTransactionIdAsync_ExistingTransactionId_ReturnsAccount()
	{
		// Arrange
		Guid transactionId = Guid.NewGuid();
		AccountEntity entity = AccountEntityGenerator.Generate();
		Account expected = AccountGenerator.Generate();

		_mockRepository.Setup(r => r.GetByTransactionIdAsync(transactionId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
		_mockMapper.Setup(m => m.Map<Account>(entity)).Returns(expected);

		// Act
		Account? actual = await _service.GetByTransactionIdAsync(transactionId, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		actual.Should().BeSameAs(expected);
	}

	[Fact]
	public async Task GetByTransactionIdAsync_NonExistingTransactionId_ReturnsNull()
	{
		// Arrange
		Guid transactionId = Guid.NewGuid();
		_mockRepository.Setup(r => r.GetByTransactionIdAsync(transactionId, It.IsAny<CancellationToken>())).ReturnsAsync((AccountEntity?)null);

		// Act
		Account? actual = await _service.GetByTransactionIdAsync(transactionId, CancellationToken.None);

		// Assert
		Assert.Null(actual);
	}

	[Fact]
	public async Task GetCountAsync_ReturnsCorrectCount()
	{
		// Arrange
		int expected = 5;
		_mockRepository.Setup(r => r.GetCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		// Act
		int actual = await _service.GetCountAsync(CancellationToken.None);

		// Assert
		Assert.Equal(expected, actual);
	}

	[Fact]
	public async Task UpdateAsync_ValidAccounts_CallsRepositoryUpdateAsync()
	{
		// Arrange
		List<Account> models = AccountGenerator.GenerateList(2);
		List<AccountEntity> entities = AccountEntityGenerator.GenerateList(2);

		_mockMapper.Setup(m => m.Map<AccountEntity>(It.IsAny<Account>())).Returns<Account>(a => entities.First());

		// Act
		await _service.UpdateAsync(models, CancellationToken.None);

		// Assert
		_mockRepository.Verify(r => r.UpdateAsync(It.IsAny<List<AccountEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
	}
}
