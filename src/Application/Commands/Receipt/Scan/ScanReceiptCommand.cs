using Application.Interfaces;

namespace Application.Commands.Receipt.Scan;

public record ScanReceiptCommand : ICommand<ScanReceiptResult>
{
	public byte[] ImageBytes { get; }
	public string ContentType { get; }

	public const string ImageBytesCannotBeEmpty = "Image bytes cannot be empty.";
	public const string ContentTypeCannotBeEmpty = "Content type cannot be empty.";

	public ScanReceiptCommand(byte[] imageBytes, string contentType)
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

		ImageBytes = imageBytes;
		ContentType = contentType;
	}
}
