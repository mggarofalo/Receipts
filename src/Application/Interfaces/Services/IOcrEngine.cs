namespace Application.Interfaces.Services;

public record OcrResult(string Text, float Confidence);

public interface IOcrEngine
{
	Task<OcrResult> ExtractTextAsync(byte[] imageBytes, CancellationToken ct);
}
