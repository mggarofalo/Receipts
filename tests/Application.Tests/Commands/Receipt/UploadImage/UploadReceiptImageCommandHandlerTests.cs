using Application.Commands.Receipt.UploadImage;
using Application.Interfaces.Services;
using FluentAssertions;
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
	private readonly Mock<IImageProcessingService> _mockProcessingService;
	private readonly UploadReceiptImageCommandHandler _handler;

	public UploadReceiptImageCommandHandlerTests()
	{
		_mockReceiptService = new Mock<IReceiptService>();
		_mockStorageService = new Mock<IImageStorageService>();
		_mockProcessingService = new Mock<IImageProcessingService>();
		_handler = new UploadReceiptImageCommandHandler(
			_mockReceiptService.Object,
			_mockStorageService.Object,
			_mockProcessingService.Object);
	}

	[Fact]
	public async Task Handle_ValidCommand_ReturnsPaths()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		byte[] imageBytes = [0xFF, 0xD8, 0xFF, 0xE0]; // JPEG magic bytes
		UploadReceiptImageCommand command = new(receiptId, imageBytes, "image/jpeg", ".jpg");

		_mockReceiptService
			.Setup(s => s.ExistsAsync(receiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		_mockStorageService
			.Setup(s => s.SaveOriginalAsync(receiptId, imageBytes, ".jpg", It.IsAny<CancellationToken>()))
			.ReturnsAsync($"{receiptId}/original.jpg");

		ImageProcessingResult processingResult = new([0x89, 0x50, 0x4E, 0x47], 100, 200);
		_mockProcessingService
			.Setup(s => s.PreprocessAsync(imageBytes, "image/jpeg", It.IsAny<CancellationToken>()))
			.ReturnsAsync(processingResult);

		_mockStorageService
			.Setup(s => s.SaveProcessedAsync(receiptId, processingResult.ProcessedBytes, It.IsAny<CancellationToken>()))
			.ReturnsAsync($"{receiptId}/processed.png");

		// Act
		UploadReceiptImageResult result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.OriginalImagePath.Should().Be($"{receiptId}/original.jpg");
		result.ProcessedImagePath.Should().Be($"{receiptId}/processed.png");

		_mockReceiptService.Verify(
			s => s.UpdateImagePathsAsync(receiptId, $"{receiptId}/original.jpg", $"{receiptId}/processed.png", It.IsAny<CancellationToken>()),
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
	}

	[Fact]
	public async Task Handle_ValidCommand_CallsServicesInOrder()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		byte[] imageBytes = [0xFF, 0xD8];
		UploadReceiptImageCommand command = new(receiptId, imageBytes, "image/png", ".png");

		_mockReceiptService
			.Setup(s => s.ExistsAsync(receiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		_mockStorageService
			.Setup(s => s.SaveOriginalAsync(receiptId, imageBytes, ".png", It.IsAny<CancellationToken>()))
			.ReturnsAsync($"{receiptId}/original.png");

		ImageProcessingResult processingResult = new([0x01], 50, 50);
		_mockProcessingService
			.Setup(s => s.PreprocessAsync(imageBytes, "image/png", It.IsAny<CancellationToken>()))
			.ReturnsAsync(processingResult);

		_mockStorageService
			.Setup(s => s.SaveProcessedAsync(receiptId, processingResult.ProcessedBytes, It.IsAny<CancellationToken>()))
			.ReturnsAsync($"{receiptId}/processed.png");

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		_mockReceiptService.Verify(s => s.ExistsAsync(receiptId, It.IsAny<CancellationToken>()), Times.Once);
		_mockStorageService.Verify(s => s.SaveOriginalAsync(receiptId, imageBytes, ".png", It.IsAny<CancellationToken>()), Times.Once);
		_mockProcessingService.Verify(s => s.PreprocessAsync(imageBytes, "image/png", It.IsAny<CancellationToken>()), Times.Once);
		_mockStorageService.Verify(s => s.SaveProcessedAsync(receiptId, processingResult.ProcessedBytes, It.IsAny<CancellationToken>()), Times.Once);
		_mockReceiptService.Verify(
			s => s.UpdateImagePathsAsync(receiptId, $"{receiptId}/original.png", $"{receiptId}/processed.png", It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task Handle_ProcessingServiceThrows_PropagatesInvalidOperationException()
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
			.ReturnsAsync($"{receiptId}/original.jpg");

		_mockProcessingService
			.Setup(s => s.PreprocessAsync(imageBytes, "image/jpeg", It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("The uploaded file is not a supported image format."));

		// Act
		Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*not a supported image format*");

		// Verify SaveProcessedAsync was never called after processing failure
		_mockStorageService.Verify(
			s => s.SaveProcessedAsync(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()),
			Times.Never);
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

		// Verify preprocessing was never called
		_mockProcessingService.Verify(
			s => s.PreprocessAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}
}
