using Application.Commands.Receipt.UploadImage;
using Application.Interfaces.Services;
using FluentAssertions;
using Moq;

namespace Application.Tests.Commands.Receipt.UploadImage;

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
}
