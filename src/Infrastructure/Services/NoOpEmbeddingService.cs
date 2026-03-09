using Application.Interfaces.Services;

namespace Infrastructure.Services;

public class NoOpEmbeddingService : IEmbeddingService
{
	public bool IsConfigured => false;

	public Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken)
	{
		return Task.FromResult(Array.Empty<float>());
	}

	public Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts, CancellationToken cancellationToken)
	{
		return Task.FromResult(new List<float[]>());
	}
}
