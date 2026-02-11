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

public class ReceiptServiceTests
{
	private readonly Mock<IReceiptRepository> _mockRepository;
	private readonly Mock<IMapper> _mockMapper;
	private readonly ReceiptService _service;

	public ReceiptServiceTests()
	{
		_mockRepository = new Mock<IReceiptRepository>();
		_mockMapper = new Mock<IMapper>();
		_service = new ReceiptService(_mockRepository.Object, _mockMapper.Object);
	}

	[Fact]
	public async Task CreateAsync_ValidReceipts_CallsRepositoryCreateAsyncAndReturnsCreatedReceipts()
	{
		// Arrange
		List<Receipt> expected = ReceiptGenerator.GenerateList(2);
		List<ReceiptEntity> entities = ReceiptEntityGenerator.GenerateList(2);
		List<ReceiptEntity> createdEntities = ReceiptEntityGenerator.GenerateList(2);

		_mockMapper.Setup(m => m.Map<ReceiptEntity>(It.IsAny<Receipt>())).Returns<Receipt>(r => entities.First());
		_mockRepository.Setup(r => r.CreateAsync(It.IsAny<List<ReceiptEntity>>(), It.IsAny<CancellationToken>())).ReturnsAsync(createdEntities);
		_mockMapper.Setup(m => m.Map<Receipt>(It.IsAny<ReceiptEntity>())).Returns<ReceiptEntity>(e => expected.First());

		// Act
		List<Receipt> actual = await _service.CreateAsync(expected, CancellationToken.None);

		// Assert
		Assert.Equal(expected.Count, actual.Count);
		_mockRepository.Verify(r => r.CreateAsync(It.IsAny<List<ReceiptEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
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
	public async Task GetAllAsync_ReturnsAllReceipts()
	{
		// Arrange
		List<ReceiptEntity> entities = ReceiptEntityGenerator.GenerateList(3);
		List<Receipt> expected = ReceiptGenerator.GenerateList(3);

		_mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entities);
		_mockMapper.Setup(m => m.Map<Receipt>(It.IsAny<ReceiptEntity>())).Returns<ReceiptEntity>(e => expected.First());

		// Act
		List<Receipt> actual = await _service.GetAllAsync(CancellationToken.None);

		// Assert
		Assert.Equal(expected.Count, actual.Count);
	}

	[Fact]
	public async Task GetByIdAsync_ExistingId_ReturnsReceipt()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		ReceiptEntity entity = ReceiptEntityGenerator.Generate();
		Receipt expected = ReceiptGenerator.Generate();

		_mockRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
		_mockMapper.Setup(m => m.Map<Receipt>(entity)).Returns(expected);

		// Act
		Receipt? actual = await _service.GetByIdAsync(id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		actual.Should().BeSameAs(expected);
	}

	[Fact]
	public async Task GetByIdAsync_NonExistingId_ReturnsNull()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		_mockRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((ReceiptEntity?)null);

		// Act
		Receipt? actual = await _service.GetByIdAsync(id, CancellationToken.None);

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
	public async Task UpdateAsync_ValidReceipts_CallsRepositoryUpdateAsync()
	{
		// Arrange
		List<Receipt> models = ReceiptGenerator.GenerateList(2);
		List<ReceiptEntity> entities = ReceiptEntityGenerator.GenerateList(2);

		_mockMapper.Setup(m => m.Map<ReceiptEntity>(It.IsAny<Receipt>())).Returns<Receipt>(r => entities.First());

		// Act
		await _service.UpdateAsync(models, CancellationToken.None);

		// Assert
		_mockRepository.Verify(r => r.UpdateAsync(It.IsAny<List<ReceiptEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
	}
}
