using Application.Commands.Receipt.UploadImage;
using Application.Interfaces.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Application.Tests.Commands.Receipt.UploadImage;

public class UploadReceiptImageCommandValidationTests
{
	[Fact]
	public void Constructor_NullImageBytes_ThrowsArgumentNullException()
	{
		// Act
		Action act = () => new UploadReceiptImageCommand(Guid.NewGuid(), null!, "image/jpeg", ".jpg");

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.And.ParamName.Should().Be("imageBytes");
	}

	[Fact]
	public void Constructor_EmptyImageBytes_ThrowsArgumentException()
	{
		// Act
		Action act = () => new UploadReceiptImageCommand(Guid.NewGuid(), [], "image/jpeg", ".jpg");

		// Assert
		act.Should().Throw<ArgumentException>()
			.WithMessage($"*{UploadReceiptImageCommand.ImageBytesCannotBeEmpty}*");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Constructor_InvalidContentType_ThrowsArgumentException(string? contentType)
	{
		// Act
		Action act = () => new UploadReceiptImageCommand(Guid.NewGuid(), [0xFF], contentType!, ".jpg");

		// Assert
		act.Should().Throw<ArgumentException>()
			.WithMessage($"*{UploadReceiptImageCommand.ContentTypeCannotBeEmpty}*");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Constructor_InvalidFileExtension_ThrowsArgumentException(string? extension)
	{
		// Act
		Action act = () => new UploadReceiptImageCommand(Guid.NewGuid(), [0xFF], "image/jpeg", extension!);

		// Assert
		act.Should().Throw<ArgumentException>()
			.WithMessage($"*{UploadReceiptImageCommand.FileExtensionCannotBeEmpty}*");
	}

	[Fact]
	public void Constructor_ValidArguments_SetsProperties()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		byte[] bytes = [0xFF, 0xD8];

		// Act
		UploadReceiptImageCommand command = new(receiptId, bytes, "image/jpeg", ".jpg");

		// Assert
		command.ReceiptId.Should().Be(receiptId);
		command.ImageBytes.Should().BeSameAs(bytes);
		command.ContentType.Should().Be("image/jpeg");
		command.FileExtension.Should().Be(".jpg");
	}
}

public class UploadReceiptImageCommandHandlerTests
{
	private readonly Mock<IReceiptService> _mockReceiptService;
	private readonly Mock<IImageStorageService> _mockStorageService;
	private readonly Mock<IImageValidationService> _mockValidationService;
	private readonly UploadReceiptImageCommandHandler _handler;

	public UploadReceiptImageCommandHandlerTests()
	{
		_mockReceiptService = new Mock<IReceiptService>();
		_mockStorageService = new Mock<IImageStorageService>();
		_mockValidationService = new Mock<IImageValidationService>();
		_handler = new UploadReceiptImageCommandHandler(
			_mockReceiptService.Object,
			_mockStorageService.Object,
			_mockValidationService.Object,
			NullLogger<UploadReceiptImageCommandHandler>.Instance);
	}

	[Fact]
	public async Task Handle_ValidCommand_ReturnsOriginalPath()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		byte[] imageBytes = [0xFF, 0xD8, 0xFF, 0xE0]; // JPEG magic bytes
		UploadReceiptImageCommand command = new(receiptId, imageBytes, "image/jpeg", ".jpg");

		string expectedPath = $"{receiptId}/original.jpg";

		_mockReceiptService
			.Setup(s => s.ExistsAsync(receiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		_mockStorageService
			.Setup(s => s.SaveOriginalAsync(receiptId, imageBytes, ".jpg", It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedPath);

		// Act
		UploadReceiptImageResult result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.OriginalImagePath.Should().Be(expectedPath);

		_mockReceiptService.Verify(
			s => s.UpdateOriginalImagePathAsync(receiptId, expectedPath, It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task Handle_ReceiptNotFound_ThrowsKeyNotFoundException()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		byte[] imageBytes = [0xFF, 0xD8, 0xFF, 0xE0];
		UploadReceiptImageCommand command = new(receiptId, imageBytes, "image/jpeg", ".jpg");

		_mockReceiptService
			.Setup(s => s.ExistsAsync(receiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<KeyNotFoundException>();

		// Nothing should have been validated or saved when the receipt is missing.
		_mockValidationService.Verify(
			s => s.ValidateAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()),
			Times.Never);
		_mockStorageService.Verify(
			s => s.SaveOriginalAsync(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task Handle_ValidCommand_CallsServicesInOrder()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		byte[] imageBytes = [0xFF, 0xD8];
		UploadReceiptImageCommand command = new(receiptId, imageBytes, "image/png", ".png");

		string expectedPath = $"{receiptId}/original.png";

		_mockReceiptService
			.Setup(s => s.ExistsAsync(receiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		_mockStorageService
			.Setup(s => s.SaveOriginalAsync(receiptId, imageBytes, ".png", It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedPath);

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		_mockReceiptService.Verify(s => s.ExistsAsync(receiptId, It.IsAny<CancellationToken>()), Times.Once);
		_mockValidationService.Verify(s => s.ValidateAsync(imageBytes, It.IsAny<CancellationToken>()), Times.Once);
		_mockStorageService.Verify(s => s.SaveOriginalAsync(receiptId, imageBytes, ".png", It.IsAny<CancellationToken>()), Times.Once);
		_mockReceiptService.Verify(
			s => s.UpdateOriginalImagePathAsync(receiptId, expectedPath, It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task Handle_ValidationServiceThrows_DoesNotSaveOrCleanUp()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		byte[] imageBytes = [0x47, 0x49, 0x46]; // GIF magic bytes — unsupported
		UploadReceiptImageCommand command = new(receiptId, imageBytes, "image/jpeg", ".jpg");

		_mockReceiptService
			.Setup(s => s.ExistsAsync(receiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		_mockValidationService
			.Setup(s => s.ValidateAsync(imageBytes, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("The uploaded file is not a supported image format. Only JPEG and PNG are accepted."));

		// Act
		Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*not a supported image format*");

		// Nothing should hit disk when validation fails up front.
		_mockStorageService.Verify(
			s => s.SaveOriginalAsync(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
			Times.Never);
		_mockStorageService.Verify(
			s => s.DeleteReceiptImagesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task Handle_UpdateImagePathsThrows_CleansUpOrphanedFiles()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		byte[] imageBytes = [0xFF, 0xD8];
		UploadReceiptImageCommand command = new(receiptId, imageBytes, "image/jpeg", ".jpg");

		string savedPath = $"{receiptId}/original.jpg";

		_mockReceiptService
			.Setup(s => s.ExistsAsync(receiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		_mockStorageService
			.Setup(s => s.SaveOriginalAsync(receiptId, imageBytes, ".jpg", It.IsAny<CancellationToken>()))
			.ReturnsAsync(savedPath);

		_mockReceiptService
			.Setup(s => s.UpdateOriginalImagePathAsync(receiptId, savedPath, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("DB offline"));

		// Act
		Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("DB offline");

		// The catch block must clean up the already-saved original.
		_mockStorageService.Verify(
			s => s.DeleteReceiptImagesAsync(receiptId, It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task Handle_StorageServiceThrows_PropagatesException()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		byte[] imageBytes = [0xFF, 0xD8];
		UploadReceiptImageCommand command = new(receiptId, imageBytes, "image/jpeg", ".jpg");

		_mockReceiptService
			.Setup(s => s.ExistsAsync(receiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		_mockStorageService
			.Setup(s => s.SaveOriginalAsync(receiptId, imageBytes, ".jpg", It.IsAny<CancellationToken>()))
			.ThrowsAsync(new IOException("Disk full"));

		// Act
		Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<IOException>()
			.WithMessage("Disk full");

		// Validation ran before SaveOriginal, but no cleanup is needed because the save never
		// produced a file.
		_mockValidationService.Verify(
			s => s.ValidateAsync(imageBytes, It.IsAny<CancellationToken>()),
			Times.Once);
		_mockStorageService.Verify(
			s => s.DeleteReceiptImagesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task Handle_PropagatesCallerCancellationTokenToEverySubservice()
	{
		// Arrange — RECEIPTS-647: the upload handler fans out across four
		// service calls (ExistsAsync, ValidateAsync, SaveOriginalAsync,
		// UpdateOriginalImagePathAsync). All four must receive the caller's
		// exact token. Mock setups use It.Is<CancellationToken>(t => t == expected)
		// so a refactor that accidentally substitutes CancellationToken.None
		// fails the verify — the cleanup-on-rollback path is excluded here
		// because it intentionally must run even if the caller cancels.
		Guid receiptId = Guid.NewGuid();
		byte[] imageBytes = [0xFF, 0xD8];
		UploadReceiptImageCommand command = new(receiptId, imageBytes, "image/jpeg", ".jpg");
		string expectedPath = $"{receiptId}/original.jpg";
		using CancellationTokenSource cts = new();
		CancellationToken expected = cts.Token;

		_mockReceiptService
			.Setup(s => s.ExistsAsync(receiptId, It.Is<CancellationToken>(t => t == expected)))
			.ReturnsAsync(true);
		_mockStorageService
			.Setup(s => s.SaveOriginalAsync(receiptId, imageBytes, ".jpg", It.Is<CancellationToken>(t => t == expected)))
			.ReturnsAsync(expectedPath);

		// Act
		await _handler.Handle(command, expected);

		// Assert — every subservice received the exact caller token
		_mockReceiptService.Verify(
			s => s.ExistsAsync(receiptId, It.Is<CancellationToken>(t => t == expected)),
			Times.Once);
		_mockValidationService.Verify(
			s => s.ValidateAsync(imageBytes, It.Is<CancellationToken>(t => t == expected)),
			Times.Once);
		_mockStorageService.Verify(
			s => s.SaveOriginalAsync(receiptId, imageBytes, ".jpg", It.Is<CancellationToken>(t => t == expected)),
			Times.Once);
		_mockReceiptService.Verify(
			s => s.UpdateOriginalImagePathAsync(receiptId, expectedPath, It.Is<CancellationToken>(t => t == expected)),
			Times.Once);
	}

	[Fact]
	public async Task Handle_UpdateImagePathsThrows_CleanupUsesNoneToken()
	{
		// Arrange — RECEIPTS-640: the cleanup path must use CancellationToken.None so a
		// caller-canceled token does not silently abort the orphan removal. Without this,
		// an upload that races a user-cancel against a path-update failure would leave
		// the saved blob orphaned on disk.
		Guid receiptId = Guid.NewGuid();
		byte[] imageBytes = [0xFF, 0xD8];
		UploadReceiptImageCommand command = new(receiptId, imageBytes, "image/jpeg", ".jpg");
		string savedPath = $"{receiptId}/original.jpg";
		using CancellationTokenSource cts = new();
		CancellationToken caller = cts.Token;

		_mockReceiptService
			.Setup(s => s.ExistsAsync(receiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);
		_mockStorageService
			.Setup(s => s.SaveOriginalAsync(receiptId, imageBytes, ".jpg", It.IsAny<CancellationToken>()))
			.ReturnsAsync(savedPath);
		_mockReceiptService
			.Setup(s => s.UpdateOriginalImagePathAsync(receiptId, savedPath, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("DB offline"));

		// Act
		Func<Task> act = () => _handler.Handle(command, caller);

		// Assert — original exception propagates; cleanup ran with CancellationToken.None
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("DB offline");

		_mockStorageService.Verify(
			s => s.DeleteReceiptImagesAsync(
				receiptId,
				It.Is<CancellationToken>(t => t == CancellationToken.None)),
			Times.Once);
	}

	[Fact]
	public async Task Handle_UpdateImagePathsThrows_CleanupAlsoThrows_PreservesOriginalException()
	{
		// Arrange — RECEIPTS-640: when both UpdateOriginalImagePathAsync and the cleanup
		// DeleteReceiptImagesAsync throw, the original exception (the actual root cause the
		// operator needs to see) must propagate, not the cleanup failure. The previous catch
		// block did NOT swallow the cleanup exception, which silently replaced the originating
		// exception in the operator's logs.
		Guid receiptId = Guid.NewGuid();
		byte[] imageBytes = [0xFF, 0xD8];
		UploadReceiptImageCommand command = new(receiptId, imageBytes, "image/jpeg", ".jpg");
		string savedPath = $"{receiptId}/original.jpg";

		_mockReceiptService
			.Setup(s => s.ExistsAsync(receiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);
		_mockStorageService
			.Setup(s => s.SaveOriginalAsync(receiptId, imageBytes, ".jpg", It.IsAny<CancellationToken>()))
			.ReturnsAsync(savedPath);
		_mockReceiptService
			.Setup(s => s.UpdateOriginalImagePathAsync(receiptId, savedPath, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("DB offline"));
		_mockStorageService
			.Setup(s => s.DeleteReceiptImagesAsync(receiptId, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new IOException("Blob storage offline"));

		// Use a captured logger so we can assert the cleanup-failure log fired.
		Mock<ILogger<UploadReceiptImageCommandHandler>> logger = new();
		UploadReceiptImageCommandHandler handler = new(
			_mockReceiptService.Object,
			_mockStorageService.Object,
			_mockValidationService.Object,
			logger.Object);

		// Act
		Func<Task> act = () => handler.Handle(command, CancellationToken.None);

		// Assert — the originating exception (DB offline) wins, not the cleanup failure
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("DB offline");

		// Cleanup was attempted (and it threw)
		_mockStorageService.Verify(
			s => s.DeleteReceiptImagesAsync(receiptId, It.IsAny<CancellationToken>()),
			Times.Once);

		// Cleanup failure was logged at Error level so the operator can still see it
		logger.Verify(
			x => x.Log(
				LogLevel.Error,
				It.IsAny<EventId>(),
				It.IsAny<It.IsAnyType>(),
				It.IsAny<Exception?>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);
	}

	[Fact]
	public async Task Handle_CallerCanceled_DoesNotTriggerCleanup()
	{
		// Arrange — RECEIPTS-640: a caller cancellation (OperationCanceledException) must NOT
		// trigger the destructive blob cleanup. Cancellation is a user choice; the saved blob
		// should remain in place so a retry can re-attach it. Previously the unfiltered
		// catch-all called DeleteReceiptImagesAsync on every exception including OCE.
		Guid receiptId = Guid.NewGuid();
		byte[] imageBytes = [0xFF, 0xD8];
		UploadReceiptImageCommand command = new(receiptId, imageBytes, "image/jpeg", ".jpg");
		string savedPath = $"{receiptId}/original.jpg";

		_mockReceiptService
			.Setup(s => s.ExistsAsync(receiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);
		_mockStorageService
			.Setup(s => s.SaveOriginalAsync(receiptId, imageBytes, ".jpg", It.IsAny<CancellationToken>()))
			.ReturnsAsync(savedPath);
		_mockReceiptService
			.Setup(s => s.UpdateOriginalImagePathAsync(receiptId, savedPath, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new OperationCanceledException("Caller canceled"));

		// Act
		Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

		// Assert — OCE propagates and no destructive cleanup ran
		await act.Should().ThrowAsync<OperationCanceledException>();

		_mockStorageService.Verify(
			s => s.DeleteReceiptImagesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}
}
