using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Services;
using Moq;
using SampleData.Domain.Core;
using SampleData.Entities;

namespace Infrastructure.Tests.Services;

public class TransactionServiceTests
{
	private readonly Mock<ITransactionRepository> _mockRepository;
	private readonly Mock<IMapper> _mockMapper;
	private readonly TransactionService _service;

	public TransactionServiceTests()
	{
		_mockRepository = new Mock<ITransactionRepository>();
		_mockMapper = new Mock<IMapper>();
		_service = new TransactionService(_mockRepository.Object, _mockMapper.Object);
	}

	[Fact]
	public async Task CreateAsync_ValidTransactions_CallsRepositoryCreateAsyncAndReturnsCreatedTransactions()
	{
		// Arrange
		List<Transaction> expected = TransactionGenerator.GenerateList(2);
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		List<TransactionEntity> entities = TransactionEntityGenerator.GenerateList(2);
		List<TransactionEntity> createdEntities = TransactionEntityGenerator.GenerateList(2);

		_mockMapper.Setup(m => m.Map<TransactionEntity>(It.IsAny<Transaction>())).Returns<Transaction>(t => entities.First());
		_mockRepository.Setup(r => r.CreateAsync(It.IsAny<List<TransactionEntity>>(), It.IsAny<CancellationToken>())).ReturnsAsync(createdEntities);
		_mockMapper.Setup(m => m.Map<Transaction>(It.IsAny<TransactionEntity>())).Returns<TransactionEntity>(e => expected.First());

		// Act
		List<Transaction> actual = await _service.CreateAsync(expected, receiptId, accountId, CancellationToken.None);

		// Assert
		Assert.Equal(expected.Count, actual.Count);
		_mockRepository.Verify(r => r.CreateAsync(It.IsAny<List<TransactionEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
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
	public async Task GetAllAsync_ReturnsAllTransactions()
	{
		// Arrange
		List<TransactionEntity> entities = TransactionEntityGenerator.GenerateList(3);
		List<Transaction> expected = TransactionGenerator.GenerateList(3);

		_mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entities);
		_mockMapper.Setup(m => m.Map<Transaction>(It.IsAny<TransactionEntity>())).Returns<TransactionEntity>(e => expected.First());

		// Act
		List<Transaction> actual = await _service.GetAllAsync(CancellationToken.None);

		// Assert
		Assert.Equal(expected.Count, actual.Count);
	}

	[Fact]
	public async Task GetByIdAsync_ExistingId_ReturnsTransaction()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		TransactionEntity entity = TransactionEntityGenerator.Generate();
		Transaction expected = TransactionGenerator.Generate();

		_mockRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
		_mockMapper.Setup(m => m.Map<Transaction>(entity)).Returns(expected);

		// Act
		Transaction? actual = await _service.GetByIdAsync(id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public async Task GetByIdAsync_NonExistingId_ReturnsNull()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		_mockRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((TransactionEntity?)null);

		// Act
		Transaction? actual = await _service.GetByIdAsync(id, CancellationToken.None);

		// Assert
		Assert.Null(actual);
	}

	[Fact]
	public async Task GetByReceiptIdAsync_ExistingReceiptId_ReturnsTransactions()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<TransactionEntity> entities = TransactionEntityGenerator.GenerateList(2);
		List<Transaction> expected = TransactionGenerator.GenerateList(2);

		_mockRepository.Setup(r => r.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>())).ReturnsAsync(entities);
		_mockMapper.Setup(m => m.Map<Transaction>(It.IsAny<TransactionEntity>())).Returns<TransactionEntity>(e => expected.First());

		// Act
		List<Transaction>? actual = await _service.GetByReceiptIdAsync(receiptId, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		Assert.Equal(expected.Count, actual.Count);
	}

	[Fact]
	public async Task GetByReceiptIdAsync_NonExistingReceiptId_ReturnsNull()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		_mockRepository.Setup(r => r.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>())).ReturnsAsync((List<TransactionEntity>?)null);

		// Act
		List<Transaction>? actual = await _service.GetByReceiptIdAsync(receiptId, CancellationToken.None);

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
	public async Task UpdateAsync_ValidTransactions_CallsRepositoryUpdateAsync()
	{
		// Arrange
		List<Transaction> models = TransactionGenerator.GenerateList(2);
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		List<TransactionEntity> entities = TransactionEntityGenerator.GenerateList(2);

		_mockMapper.Setup(m => m.Map<TransactionEntity>(It.IsAny<Transaction>())).Returns<Transaction>(t => entities.First());

		// Act
		await _service.UpdateAsync(models, receiptId, accountId, CancellationToken.None);

		// Assert
		_mockRepository.Verify(r => r.UpdateAsync(It.Is<List<TransactionEntity>>(e =>
			e.All(t => t.ReceiptId == receiptId && t.AccountId == accountId)),
			It.IsAny<CancellationToken>()), Times.Once);
	}
}
