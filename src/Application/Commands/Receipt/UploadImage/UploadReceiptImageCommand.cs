using Application.Interfaces;

namespace Application.Commands.Receipt.UploadImage;

public record UploadReceiptImageCommand : ICommand<UploadReceiptImageResult>
{
	public Guid ReceiptId { get; }
	public byte[] ImageBytes { get; }
	public string ContentType { get; }
	public string FileExtension { get; }

	public const string ImageBytesCannotBeEmpty = "Image bytes cannot be empty.";
	public const string ContentTypeCannotBeEmpty = "Content type cannot be empty.";
	public const string FileExtensionCannotBeEmpty = "File extension cannot be empty.";

	public UploadReceiptImageCommand(Guid receiptId, byte[] imageBytes, string contentType, string fileExtension)
	{
		ArgumentNullException.ThrowIfNull(imageBytes);

		if (imageBytes.Length == 0)
		{
			throw new ArgumentException(ImageBytesCannotBeEmpty, nameof(imageBytes));
		}

		if (string.IsNullOrWhiteSpace(contentType))
		{
			throw new ArgumentException(ContentTypeCannotBeEmpty, nameof(contentType));
		}

		if (string.IsNullOrWhiteSpace(fileExtension))
		{
			throw new ArgumentException(FileExtensionCannotBeEmpty, nameof(fileExtension));
		}

		ReceiptId = receiptId;
		ImageBytes = imageBytes;
		ContentType = contentType;
		FileExtension = fileExtension;
	}
}
