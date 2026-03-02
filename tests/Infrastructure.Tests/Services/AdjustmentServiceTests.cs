using Domain.Core;
using FluentAssertions;
using Infrastructure.Entities.Core;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Mapping;
using Infrastructure.Services;
using Moq;
using SampleData.Domain.Core;
using SampleData.Entities;

namespace Infrastructure.Tests.Services;

public class AdjustmentServiceTests
{
	private readonly Mock<IAdjustmentRepository> _mockRepository;
	private readonly AdjustmentMapper _mapper;
	private readonly AdjustmentService _service;

	public AdjustmentServiceTests()
	{
		_mockRepository = new Mock<IAdjustmentRepository>();
		_mapper = new AdjustmentMapper();
		_service = new AdjustmentService(_mockRepository.Object, _mapper);
	}

	[Fact]
	public async Task CreateAsync_ValidAdjustments_CallsRepositoryAndReturnsCreated()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<Adjustment> models = AdjustmentGenerator.GenerateList(2);
		List<AdjustmentEntity> createdEntities = AdjustmentEntityGenerator.GenerateList(2);

		_mockRepository.Setup(r => r.CreateAsync(It.IsAny<List<AdjustmentEntity>>(), It.IsAny<CancellationToken>())).ReturnsAsync(createdEntities);

		// Act
		List<Adjustment> actual = await _service.CreateAsync(models, receiptId, CancellationToken.None);

		// Assert
		Assert.Equal(createdEntities.Count, actual.Count);
		_mockRepository.Verify(r => r.CreateAsync(
			It.Is<List<AdjustmentEntity>>(entities => entities.All(e => e.ReceiptId == receiptId)),
			It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task DeleteAsync_ValidIds_CallsRepository()
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
		_mockRepository.Setup(r => r.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

		// Act
		bool actual = await _service.ExistsAsync(id, CancellationToken.None);

		// Assert
		Assert.True(actual);
	}

	[Fact]
	public async Task GetAllAsync_ReturnsAllAdjustments()
	{
		// Arrange
		List<AdjustmentEntity> entities = AdjustmentEntityGenerator.GenerateList(3);
		_mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entities);

		// Act
		List<Adjustment> actual = await _service.GetAllAsync(CancellationToken.None);

		// Assert
		Assert.Equal(entities.Count, actual.Count);
	}

	[Fact]
	public async Task GetDeletedAsync_ReturnsDeletedAdjustments()
	{
		// Arrange
		List<AdjustmentEntity> entities = AdjustmentEntityGenerator.GenerateList(2);
		_mockRepository.Setup(r => r.GetDeletedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entities);

		// Act
		List<Adjustment> actual = await _service.GetDeletedAsync(CancellationToken.None);

		// Assert
		Assert.Equal(entities.Count, actual.Count);
	}

	[Fact]
	public async Task GetByIdAsync_ExistingId_ReturnsAdjustment()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		AdjustmentEntity entity = AdjustmentEntityGenerator.Generate();
		_mockRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

		// Act
		Adjustment? actual = await _service.GetByIdAsync(id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		actual.Id.Should().Be(entity.Id);
	}

	[Fact]
	public async Task GetByIdAsync_NonExistingId_ReturnsNull()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		_mockRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((AdjustmentEntity?)null);

		// Act
		Adjustment? actual = await _service.GetByIdAsync(id, CancellationToken.None);

		// Assert
		Assert.Null(actual);
	}

	[Fact]
	public async Task GetByReceiptIdAsync_ExistingReceiptId_ReturnsAdjustments()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<AdjustmentEntity> entities = AdjustmentEntityGenerator.GenerateList(2);
		_mockRepository.Setup(r => r.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>())).ReturnsAsync(entities);

		// Act
		List<Adjustment>? actual = await _service.GetByReceiptIdAsync(receiptId, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		Assert.Equal(entities.Count, actual.Count);
	}

	[Fact]
	public async Task GetByReceiptIdAsync_NonExistingReceiptId_ReturnsNull()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		_mockRepository.Setup(r => r.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>())).ReturnsAsync((List<AdjustmentEntity>?)null);

		// Act
		List<Adjustment>? actual = await _service.GetByReceiptIdAsync(receiptId, CancellationToken.None);

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
	public async Task UpdateAsync_ValidAdjustments_CallsRepositoryWithReceiptId()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<Adjustment> models = AdjustmentGenerator.GenerateList(2);

		// Act
		await _service.UpdateAsync(models, receiptId, CancellationToken.None);

		// Assert
		_mockRepository.Verify(r => r.UpdateAsync(
			It.Is<List<AdjustmentEntity>>(entities => entities.All(e => e.ReceiptId == receiptId)),
			It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task RestoreAsync_ExistingId_ReturnsTrue()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		_mockRepository.Setup(r => r.RestoreAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

		// Act
		bool actual = await _service.RestoreAsync(id, CancellationToken.None);

		// Assert
		Assert.True(actual);
	}

	[Fact]
	public async Task RestoreAsync_NonExistingId_ReturnsFalse()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		_mockRepository.Setup(r => r.RestoreAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

		// Act
		bool actual = await _service.RestoreAsync(id, CancellationToken.None);

		// Assert
		Assert.False(actual);
	}
}
