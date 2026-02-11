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

public class ReceiptItemServiceTests
{
	private readonly Mock<IReceiptItemRepository> _mockRepository;
	private readonly Mock<IMapper> _mockMapper;
	private readonly ReceiptItemService _service;

	public ReceiptItemServiceTests()
	{
		_mockRepository = new Mock<IReceiptItemRepository>();
		_mockMapper = new Mock<IMapper>();
		_service = new ReceiptItemService(_mockRepository.Object, _mockMapper.Object);
	}

	[Fact]
	public async Task CreateAsync_ValidReceiptItems_CallsRepositoryCreateAsyncAndReturnsCreatedReceiptItems()
	{
		// Arrange
		List<ReceiptItem> expected = ReceiptItemGenerator.GenerateList(2);
		Guid receiptId = Guid.NewGuid();
		List<ReceiptItemEntity> entities = ReceiptItemEntityGenerator.GenerateList(2);
		List<ReceiptItemEntity> createdEntities = ReceiptItemEntityGenerator.GenerateList(2);

		_mockMapper.Setup(m => m.Map<ReceiptItemEntity>(It.IsAny<ReceiptItem>())).Returns<ReceiptItem>(r => entities.First());
		_mockRepository.Setup(r => r.CreateAsync(It.IsAny<List<ReceiptItemEntity>>(), It.IsAny<CancellationToken>())).ReturnsAsync(createdEntities);
		_mockMapper.Setup(m => m.Map<ReceiptItem>(It.IsAny<ReceiptItemEntity>())).Returns<ReceiptItemEntity>(e => expected.First());

		// Act
		List<ReceiptItem> actual = await _service.CreateAsync(expected, receiptId, CancellationToken.None);

		// Assert
		Assert.Equal(expected.Count, actual.Count);
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
	public async Task GetAllAsync_ReturnsAllReceiptItems()
	{
		// Arrange
		List<ReceiptItemEntity> entities = ReceiptItemEntityGenerator.GenerateList(3);
		List<ReceiptItem> expected = ReceiptItemGenerator.GenerateList(3);

		_mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entities);
		_mockMapper.Setup(m => m.Map<ReceiptItem>(It.IsAny<ReceiptItemEntity>())).Returns<ReceiptItemEntity>(e => expected.First());

		// Act
		List<ReceiptItem> actual = await _service.GetAllAsync(CancellationToken.None);

		// Assert
		Assert.Equal(expected.Count, actual.Count);
	}

	[Fact]
	public async Task GetByIdAsync_ExistingId_ReturnsReceiptItem()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		ReceiptItemEntity entity = ReceiptItemEntityGenerator.Generate();
		ReceiptItem expected = ReceiptItemGenerator.Generate();

		_mockRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
		_mockMapper.Setup(m => m.Map<ReceiptItem>(entity)).Returns(expected);

		// Act
		ReceiptItem? actual = await _service.GetByIdAsync(id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		actual.Should().BeSameAs(expected);
	}

	[Fact]
	public async Task GetByIdAsync_NonExistingId_ReturnsNull()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		_mockRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((ReceiptItemEntity?)null);

		// Act
		ReceiptItem? actual = await _service.GetByIdAsync(id, CancellationToken.None);

		// Assert
		Assert.Null(actual);
	}

	[Fact]
	public async Task GetByReceiptIdAsync_ExistingReceiptId_ReturnsReceiptItems()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<ReceiptItemEntity> entities = ReceiptItemEntityGenerator.GenerateList(2);
		List<ReceiptItem> expected = ReceiptItemGenerator.GenerateList(2);

		_mockRepository.Setup(r => r.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>())).ReturnsAsync(entities);
		_mockMapper.Setup(m => m.Map<ReceiptItem>(It.IsAny<ReceiptItemEntity>())).Returns<ReceiptItemEntity>(e => expected.First());

		// Act
		List<ReceiptItem>? actual = await _service.GetByReceiptIdAsync(receiptId, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		Assert.Equal(expected.Count, actual.Count);
	}

	[Fact]
	public async Task GetByReceiptIdAsync_NonExistingReceiptId_ReturnsNull()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		_mockRepository.Setup(r => r.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>())).ReturnsAsync((List<ReceiptItemEntity>?)null);

		// Act
		List<ReceiptItem>? actual = await _service.GetByReceiptIdAsync(receiptId, CancellationToken.None);

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
	public async Task UpdateAsync_ValidReceiptItems_CallsRepositoryUpdateAsync()
	{
		// Arrange
		List<ReceiptItem> models = ReceiptItemGenerator.GenerateList(2);
		Guid receiptId = Guid.NewGuid();
		List<ReceiptItemEntity> entities = ReceiptItemEntityGenerator.GenerateList(2);

		_mockMapper.Setup(m => m.Map<ReceiptItemEntity>(It.IsAny<ReceiptItem>())).Returns<ReceiptItem>(r => entities.First());

		// Act
		await _service.UpdateAsync(models, receiptId, CancellationToken.None);

		// Assert
		_mockRepository.Verify(r => r.UpdateAsync(It.Is<List<ReceiptItemEntity>>(e => e.All(ri => ri.ReceiptId == receiptId)), It.IsAny<CancellationToken>()), Times.Once);
	}
}
