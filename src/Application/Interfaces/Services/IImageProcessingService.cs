namespace Application.Interfaces.Services;

public interface IImageProcessingService
{
	Task<ImageProcessingResult> PreprocessAsync(byte[] imageBytes, string contentType, CancellationToken ct);
}

public record ImageProcessingResult(byte[] ProcessedBytes, int Width, int Height);
