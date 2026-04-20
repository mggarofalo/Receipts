using Application.Interfaces.Services;
using Common;
using Domain.NormalizedDescriptions;
using FluentAssertions;
using Infrastructure.Entities.Core;
using Infrastructure.Mapping;
using Infrastructure.Services;
using Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Pgvector;

namespace Infrastructure.Tests.Services;

[Trait("Category", "Unit")]
public class NormalizedDescriptionServiceTests
{
	private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
	private readonly Mock<IEmbeddingService> _embeddingServiceMock;
	private readonly NormalizedDescriptionMapper _mapper;

	public NormalizedDescriptionServiceTests()
	{
		(_contextFactory, MockCurrentUserAccessor accessor) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		accessor.UserId = "test-user";
		_embeddingServiceMock = new Mock<IEmbeddingService>();
		_mapper = new NormalizedDescriptionMapper();
	}

	[Fact]
	public async Task GetOrCreateAsync_ExactCaseInsensitiveMatch_ReturnsExisting()
	{
		// Arrange — seed an existing canonical entry.
		Guid existingId = Guid.NewGuid();
		using (ApplicationDbContext seed = _contextFactory.CreateDbContext())
		{
			seed.NormalizedDescriptions.Add(new NormalizedDescriptionEntity
			{
				Id = existingId,
				CanonicalName = "Organic Milk",
				Status = NormalizedDescriptionStatus.Active,
				CreatedAt = DateTimeOffset.UtcNow,
			});
			await seed.SaveChangesAsync();
		}

		_embeddingServiceMock.Setup(e => e.IsConfigured).Returns(true);
		NormalizedDescriptionService service = new(_contextFactory, _embeddingServiceMock.Object, _mapper);

		// Act — query with different casing; it should short-circuit without generating an embedding.
		NormalizedDescription result = await service.GetOrCreateAsync("organic MILK", CancellationToken.None);

		// Assert
		result.Id.Should().Be(existingId);
		result.CanonicalName.Should().Be("Organic Milk");
		_embeddingServiceMock.Verify(
			e => e.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task GetOrCreateAsync_EmbeddingServiceUnavailable_CreatesActiveEntryWithNoEmbedding()
	{
		// Arrange
		_embeddingServiceMock.Setup(e => e.IsConfigured).Returns(false);
		NormalizedDescriptionService service = new(_contextFactory, _embeddingServiceMock.Object, _mapper);

		// Act
		NormalizedDescription result = await service.GetOrCreateAsync("New Item", CancellationToken.None);

		// Assert — a new Active row was created, and no embedding was generated.
		result.Status.Should().Be(NormalizedDescriptionStatus.Active);
		result.CanonicalName.Should().Be("New Item");
		using ApplicationDbContext verify = _contextFactory.CreateDbContext();
		NormalizedDescriptionEntity stored = await verify.NormalizedDescriptions.SingleAsync(e => e.Id == result.Id);
		stored.Embedding.Should().BeNull();
		stored.EmbeddingModelVersion.Should().BeNull();
		_embeddingServiceMock.Verify(
			e => e.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task GetOrCreateAsync_AboveAutoAcceptThreshold_ReturnsAnnMatchWithoutInserting()
	{
		// Arrange — seed an existing Active entry that the fake ANN will return as the top-1.
		Guid matchedId = Guid.NewGuid();
		using (ApplicationDbContext seed = _contextFactory.CreateDbContext())
		{
			seed.NormalizedDescriptions.Add(new NormalizedDescriptionEntity
			{
				Id = matchedId,
				CanonicalName = "Gallon of Milk",
				Status = NormalizedDescriptionStatus.Active,
				CreatedAt = DateTimeOffset.UtcNow,
			});
			await seed.SaveChangesAsync();
		}

		_embeddingServiceMock.Setup(e => e.IsConfigured).Returns(true);
		_embeddingServiceMock
			.Setup(e => e.GenerateEmbeddingAsync("Whole Milk", It.IsAny<CancellationToken>()))
			.ReturnsAsync(CreateFakeEmbedding());

		TestableNormalizedDescriptionService service = new(
			_contextFactory,
			_embeddingServiceMock.Object,
			_mapper,
			matchedId,
			similarity: NormalizedDescriptionService.AutoAcceptThreshold + 0.01);

		// Act
		NormalizedDescription result = await service.GetOrCreateAsync("Whole Milk", CancellationToken.None);

		// Assert — returned the ANN match; no new row inserted.
		result.Id.Should().Be(matchedId);
		result.CanonicalName.Should().Be("Gallon of Milk");
		using ApplicationDbContext verify = _contextFactory.CreateDbContext();
		int count = await verify.NormalizedDescriptions.CountAsync();
		count.Should().Be(1);
	}

	[Fact]
	public async Task GetOrCreateAsync_BetweenThresholds_CreatesPendingReviewWithInputText()
	{
		// Arrange — seed a close-but-not-exact match that the fake ANN will return.
		Guid matchedId = Guid.NewGuid();
		using (ApplicationDbContext seed = _contextFactory.CreateDbContext())
		{
			seed.NormalizedDescriptions.Add(new NormalizedDescriptionEntity
			{
				Id = matchedId,
				CanonicalName = "Gallon of Milk",
				Status = NormalizedDescriptionStatus.Active,
				CreatedAt = DateTimeOffset.UtcNow,
			});
			await seed.SaveChangesAsync();
		}

		_embeddingServiceMock.Setup(e => e.IsConfigured).Returns(true);
		_embeddingServiceMock
			.Setup(e => e.GenerateEmbeddingAsync("Milky Thing", It.IsAny<CancellationToken>()))
			.ReturnsAsync(CreateFakeEmbedding());

		double between = (NormalizedDescriptionService.AutoAcceptThreshold + NormalizedDescriptionService.PendingReviewThreshold) / 2;
		TestableNormalizedDescriptionService service = new(
			_contextFactory,
			_embeddingServiceMock.Object,
			_mapper,
			matchedId,
			similarity: between);

		// Act
		NormalizedDescription result = await service.GetOrCreateAsync("Milky Thing", CancellationToken.None);

		// Assert — a new PendingReview row was created with the input text as canonical name.
		result.Status.Should().Be(NormalizedDescriptionStatus.PendingReview);
		result.CanonicalName.Should().Be("Milky Thing");
		result.Id.Should().NotBe(matchedId);
		using ApplicationDbContext verify = _contextFactory.CreateDbContext();
		int count = await verify.NormalizedDescriptions.CountAsync();
		count.Should().Be(2);
	}

	[Fact]
	public async Task GetOrCreateAsync_BelowPendingReviewThreshold_CreatesActiveEntry()
	{
		// Arrange — seed an unrelated Active entry that the fake ANN will return as the top-1 match
		// but with a similarity below the PendingReview threshold.
		Guid matchedId = Guid.NewGuid();
		using (ApplicationDbContext seed = _contextFactory.CreateDbContext())
		{
			seed.NormalizedDescriptions.Add(new NormalizedDescriptionEntity
			{
				Id = matchedId,
				CanonicalName = "Unrelated Item",
				Status = NormalizedDescriptionStatus.Active,
				CreatedAt = DateTimeOffset.UtcNow,
			});
			await seed.SaveChangesAsync();
		}

		_embeddingServiceMock.Setup(e => e.IsConfigured).Returns(true);
		_embeddingServiceMock
			.Setup(e => e.GenerateEmbeddingAsync("Totally Different", It.IsAny<CancellationToken>()))
			.ReturnsAsync(CreateFakeEmbedding());

		TestableNormalizedDescriptionService service = new(
			_contextFactory,
			_embeddingServiceMock.Object,
			_mapper,
			matchedId,
			similarity: NormalizedDescriptionService.PendingReviewThreshold - 0.1);

		// Act
		NormalizedDescription result = await service.GetOrCreateAsync("Totally Different", CancellationToken.None);

		// Assert — a new Active entry was created with the input text as canonical.
		result.Status.Should().Be(NormalizedDescriptionStatus.Active);
		result.CanonicalName.Should().Be("Totally Different");
		using ApplicationDbContext verify = _contextFactory.CreateDbContext();
		int count = await verify.NormalizedDescriptions.CountAsync();
		count.Should().Be(2);
	}

	[Fact]
	public async Task GetOrCreateAsync_EmptyOrWhitespace_Throws()
	{
		// Arrange
		NormalizedDescriptionService service = new(_contextFactory, _embeddingServiceMock.Object, _mapper);

		// Act + Assert
		await service.Invoking(s => s.GetOrCreateAsync("   ", CancellationToken.None))
			.Should().ThrowAsync<ArgumentException>();
	}

	[Fact]
	public async Task MergeAsync_ReLinksReceiptItemsAndDeletesDiscard()
	{
		// Arrange — two NormalizedDescriptions plus two ReceiptItems pointing at the discard entry.
		Guid keepId = Guid.NewGuid();
		Guid discardId = Guid.NewGuid();
		Guid itemAId = Guid.NewGuid();
		Guid itemBId = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		using (ApplicationDbContext seed = _contextFactory.CreateDbContext())
		{
			seed.NormalizedDescriptions.AddRange(
				new NormalizedDescriptionEntity { Id = keepId, CanonicalName = "Milk", Status = NormalizedDescriptionStatus.Active, CreatedAt = DateTimeOffset.UtcNow },
				new NormalizedDescriptionEntity { Id = discardId, CanonicalName = "Mlik", Status = NormalizedDescriptionStatus.Active, CreatedAt = DateTimeOffset.UtcNow });
			seed.ReceiptItems.AddRange(
				BuildReceiptItem(itemAId, receiptId, "Mlik", discardId),
				BuildReceiptItem(itemBId, receiptId, "Mlik", discardId));
			await seed.SaveChangesAsync();
		}

		NormalizedDescriptionService service = new(_contextFactory, _embeddingServiceMock.Object, _mapper);

		// Act
		int moved = await service.MergeAsync(keepId, discardId, CancellationToken.None);

		// Assert
		moved.Should().Be(2);
		using ApplicationDbContext verify = _contextFactory.CreateDbContext();
		(await verify.NormalizedDescriptions.AnyAsync(e => e.Id == discardId)).Should().BeFalse();
		(await verify.NormalizedDescriptions.AnyAsync(e => e.Id == keepId)).Should().BeTrue();
		List<Guid?> linkedIds = await verify.ReceiptItems
			.IgnoreAutoIncludes()
			.Where(r => r.Id == itemAId || r.Id == itemBId)
			.Select(r => r.NormalizedDescriptionId)
			.ToListAsync();
		linkedIds.Should().OnlyContain(id => id == keepId);
	}

	[Fact]
	public async Task SplitAsync_CreatesNewActiveEntryAndRepointsReceiptItem()
	{
		// Arrange — an existing NormalizedDescription shared by a ReceiptItem.
		Guid sharedId = Guid.NewGuid();
		Guid itemId = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		using (ApplicationDbContext seed = _contextFactory.CreateDbContext())
		{
			seed.NormalizedDescriptions.Add(new NormalizedDescriptionEntity
			{
				Id = sharedId,
				CanonicalName = "Shared Name",
				Status = NormalizedDescriptionStatus.Active,
				CreatedAt = DateTimeOffset.UtcNow,
			});
			seed.ReceiptItems.Add(BuildReceiptItem(itemId, receiptId, "Specific Raw Text", sharedId));
			await seed.SaveChangesAsync();
		}

		_embeddingServiceMock.Setup(e => e.IsConfigured).Returns(false);
		NormalizedDescriptionService service = new(_contextFactory, _embeddingServiceMock.Object, _mapper);

		// Act
		NormalizedDescription created = await service.SplitAsync(itemId, CancellationToken.None);

		// Assert — a new Active entry was created with the ReceiptItem's raw text, and the
		// ReceiptItem now points at the new entry.
		created.Status.Should().Be(NormalizedDescriptionStatus.Active);
		created.CanonicalName.Should().Be("Specific Raw Text");
		created.Id.Should().NotBe(sharedId);
		using ApplicationDbContext verify = _contextFactory.CreateDbContext();
		ReceiptItemEntity updatedItem = await verify.ReceiptItems
			.IgnoreAutoIncludes()
			.SingleAsync(r => r.Id == itemId);
		updatedItem.NormalizedDescriptionId.Should().Be(created.Id);
	}

	[Fact]
	public async Task UpdateStatusAsync_ChangesPendingReviewToActive()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		using (ApplicationDbContext seed = _contextFactory.CreateDbContext())
		{
			seed.NormalizedDescriptions.Add(new NormalizedDescriptionEntity
			{
				Id = id,
				CanonicalName = "Pending Entry",
				Status = NormalizedDescriptionStatus.PendingReview,
				CreatedAt = DateTimeOffset.UtcNow,
			});
			await seed.SaveChangesAsync();
		}

		NormalizedDescriptionService service = new(_contextFactory, _embeddingServiceMock.Object, _mapper);

		// Act
		bool changed = await service.UpdateStatusAsync(id, NormalizedDescriptionStatus.Active, CancellationToken.None);

		// Assert
		changed.Should().BeTrue();
		using ApplicationDbContext verify = _contextFactory.CreateDbContext();
		NormalizedDescriptionEntity stored = await verify.NormalizedDescriptions.SingleAsync(e => e.Id == id);
		stored.Status.Should().Be(NormalizedDescriptionStatus.Active);
	}

	[Fact]
	public async Task UpdateStatusAsync_NoChange_ReturnsFalse()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		using (ApplicationDbContext seed = _contextFactory.CreateDbContext())
		{
			seed.NormalizedDescriptions.Add(new NormalizedDescriptionEntity
			{
				Id = id,
				CanonicalName = "Already Active",
				Status = NormalizedDescriptionStatus.Active,
				CreatedAt = DateTimeOffset.UtcNow,
			});
			await seed.SaveChangesAsync();
		}

		NormalizedDescriptionService service = new(_contextFactory, _embeddingServiceMock.Object, _mapper);

		// Act
		bool changed = await service.UpdateStatusAsync(id, NormalizedDescriptionStatus.Active, CancellationToken.None);

		// Assert
		changed.Should().BeFalse();
	}

	[Fact]
	public async Task GetByIdAsync_ReturnsEntity_WhenPresent()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		using (ApplicationDbContext seed = _contextFactory.CreateDbContext())
		{
			seed.NormalizedDescriptions.Add(new NormalizedDescriptionEntity
			{
				Id = id,
				CanonicalName = "Findable",
				Status = NormalizedDescriptionStatus.Active,
				CreatedAt = DateTimeOffset.UtcNow,
			});
			await seed.SaveChangesAsync();
		}

		NormalizedDescriptionService service = new(_contextFactory, _embeddingServiceMock.Object, _mapper);

		// Act
		NormalizedDescription? result = await service.GetByIdAsync(id, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result!.CanonicalName.Should().Be("Findable");
	}

	[Fact]
	public async Task GetAllAsync_FilterByStatus_ReturnsOnlyMatching()
	{
		// Arrange
		using (ApplicationDbContext seed = _contextFactory.CreateDbContext())
		{
			seed.NormalizedDescriptions.AddRange(
				new NormalizedDescriptionEntity { Id = Guid.NewGuid(), CanonicalName = "Active A", Status = NormalizedDescriptionStatus.Active, CreatedAt = DateTimeOffset.UtcNow },
				new NormalizedDescriptionEntity { Id = Guid.NewGuid(), CanonicalName = "Pending B", Status = NormalizedDescriptionStatus.PendingReview, CreatedAt = DateTimeOffset.UtcNow },
				new NormalizedDescriptionEntity { Id = Guid.NewGuid(), CanonicalName = "Active C", Status = NormalizedDescriptionStatus.Active, CreatedAt = DateTimeOffset.UtcNow });
			await seed.SaveChangesAsync();
		}

		NormalizedDescriptionService service = new(_contextFactory, _embeddingServiceMock.Object, _mapper);

		// Act
		List<NormalizedDescription> pending = await service.GetAllAsync(NormalizedDescriptionStatus.PendingReview, CancellationToken.None);
		List<NormalizedDescription> all = await service.GetAllAsync(null, CancellationToken.None);

		// Assert
		pending.Should().ContainSingle();
		pending[0].CanonicalName.Should().Be("Pending B");
		all.Should().HaveCount(3);
	}

	private static ReceiptItemEntity BuildReceiptItem(Guid id, Guid receiptId, string description, Guid? normalizedId)
	{
		return new ReceiptItemEntity
		{
			Id = id,
			ReceiptId = receiptId,
			Description = description,
			Quantity = 1,
			UnitPrice = 1,
			UnitPriceCurrency = Currency.USD,
			TotalAmount = 1,
			TotalAmountCurrency = Currency.USD,
			Category = "Groceries",
			NormalizedDescriptionId = normalizedId,
		};
	}

	private static float[] CreateFakeEmbedding()
	{
		float[] embedding = new float[OnnxEmbeddingService.EmbeddingDimension];
		Random rng = new(42);
		for (int i = 0; i < embedding.Length; i++)
		{
			embedding[i] = (float)(rng.NextDouble() * 2 - 1);
		}

		return embedding;
	}

	// Test subclass that overrides the ANN search to deterministically return a seeded match.
	// This lets us exercise the threshold-band logic against InMemory, which cannot run the
	// pgvector `<=>` operator.
	private sealed class TestableNormalizedDescriptionService(
		IDbContextFactory<ApplicationDbContext> contextFactory,
		IEmbeddingService embeddingService,
		NormalizedDescriptionMapper mapper,
		Guid matchId,
		double similarity) : NormalizedDescriptionService(contextFactory, embeddingService, mapper)
	{
		private readonly Guid _matchId = matchId;
		private readonly double _similarity = similarity;

		protected override async Task<(NormalizedDescriptionEntity? Match, double? Similarity)> AnnSearchTopOneAsync(
			ApplicationDbContext context, Vector queryVector, CancellationToken cancellationToken)
		{
			NormalizedDescriptionEntity? match = await context.NormalizedDescriptions
				.FirstOrDefaultAsync(e => e.Id == _matchId, cancellationToken);
			return (match, _similarity);
		}
	}
}
