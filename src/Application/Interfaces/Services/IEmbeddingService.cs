namespace Application.Interfaces.Services;

public interface IEmbeddingService
{
	Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken);
	Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts, CancellationToken cancellationToken);
	bool IsConfigured { get; }
}
