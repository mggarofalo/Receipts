using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Interfaces.Services;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class OpenAiEmbeddingService(
	HttpClient httpClient,
	IConfiguration configuration,
	ILogger<OpenAiEmbeddingService> logger) : IEmbeddingService
{
	public bool IsConfigured => httpClient.DefaultRequestHeaders.Authorization?.Parameter is { Length: > 0 };

	public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken)
	{
		List<float[]> results = await GenerateEmbeddingsAsync([text], cancellationToken);
		return results[0];
	}

	public async Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts, CancellationToken cancellationToken)
	{
		string model = configuration[ConfigurationVariables.OpenAiEmbeddingModel]
			?? ConfigurationVariables.OpenAiDefaultEmbeddingModel;

		EmbeddingRequest request = new()
		{
			Input = texts,
			Model = model,
		};

		HttpResponseMessage response = await httpClient.PostAsJsonAsync(
			"v1/embeddings", request, cancellationToken);

		if (!response.IsSuccessStatusCode)
		{
			string body = await response.Content.ReadAsStringAsync(cancellationToken);
			string truncatedBody = body.Length > 200 ? body[..200] : body;
			logger.LogWarning("OpenAI embedding request failed with {StatusCode}: {Body}",
				response.StatusCode, truncatedBody);
			throw new HttpRequestException($"OpenAI API returned {response.StatusCode}");
		}

		EmbeddingResponse? result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(cancellationToken);

		if (result?.Data is null || result.Data.Count != texts.Count)
		{
			throw new InvalidOperationException("OpenAI returned unexpected embedding response structure");
		}

		return result.Data
			.OrderBy(d => d.Index)
			.Select(d => d.Embedding)
			.ToList();
	}

	private sealed class EmbeddingRequest
	{
		[JsonPropertyName("input")]
		public List<string> Input { get; set; } = [];

		[JsonPropertyName("model")]
		public string Model { get; set; } = string.Empty;
	}

	private sealed class EmbeddingResponse
	{
		[JsonPropertyName("data")]
		public List<EmbeddingData> Data { get; set; } = [];
	}

	private sealed class EmbeddingData
	{
		[JsonPropertyName("index")]
		public int Index { get; set; }

		[JsonPropertyName("embedding")]
		public float[] Embedding { get; set; } = [];
	}
}
