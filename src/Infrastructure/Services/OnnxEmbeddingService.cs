using Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;

namespace Infrastructure.Services;

public sealed class OnnxEmbeddingService : IEmbeddingService, IDisposable
{
	public const string ModelName = "all-MiniLM-L6-v2";
	public const int EmbeddingDimension = 384;
	private const int MaxTokens = 256;

	private readonly InferenceSession _session;
	private readonly BertTokenizer _tokenizer;
	private readonly ILogger<OnnxEmbeddingService> _logger;
	private readonly object _inferLock = new();
	private bool _disposed;

	public OnnxEmbeddingService(ILogger<OnnxEmbeddingService> logger)
	{
		_logger = logger;

		string baseDir = AppContext.BaseDirectory;
		string modelPath = Path.Combine(baseDir, "Models", "AllMiniLmL6V2", "model.onnx");
		string vocabPath = Path.Combine(baseDir, "Models", "AllMiniLmL6V2", "vocab.txt");

		if (!File.Exists(modelPath) || !File.Exists(vocabPath))
		{
			throw new FileNotFoundException(
				$"ONNX model files not found at {Path.GetDirectoryName(modelPath)}. " +
				"Run scripts/download-onnx-model.cs to download them.");
		}

		SessionOptions options = new();
		options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
		_session = new InferenceSession(modelPath, options);

		using FileStream vocabStream = File.OpenRead(vocabPath);
		_tokenizer = BertTokenizer.Create(vocabStream);
	}

	public bool IsConfigured => true;

	public Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken)
	{
		float[] embedding = GenerateEmbedding(text);
		return Task.FromResult(embedding);
	}

	public Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts, CancellationToken cancellationToken)
	{
		List<float[]> results = new(texts.Count);
		foreach (string text in texts)
		{
			cancellationToken.ThrowIfCancellationRequested();
			results.Add(GenerateEmbedding(text));
		}

		return Task.FromResult(results);
	}

	private float[] GenerateEmbedding(string text)
	{
		// Lock to guarantee thread safety: BertTokenizer's thread-safety is undocumented,
		// and this singleton may be called concurrently from the background service and request pipeline.
		// InferenceSession.Run is thread-safe per ONNX Runtime docs, but we lock the whole method
		// to keep it simple — embedding generation is I/O-bound, not a hot path.
		lock (_inferLock)
		{
			return GenerateEmbeddingCore(text);
		}
	}

	private float[] GenerateEmbeddingCore(string text)
	{
		// Tokenize: EncodeToIds with addSpecialTokens=true adds [CLS] and [SEP]
		IReadOnlyList<int> tokenIds = _tokenizer.EncodeToIds(text, MaxTokens, out _, out _);

		int seqLen = tokenIds.Count;

		// Build tensors: input_ids, attention_mask (all 1s), token_type_ids (all 0s for single sentence)
		DenseTensor<long> inputIdsTensor = new([1, seqLen]);
		DenseTensor<long> attentionMaskTensor = new([1, seqLen]);
		DenseTensor<long> tokenTypeIdsTensor = new([1, seqLen]);

		for (int i = 0; i < seqLen; i++)
		{
			inputIdsTensor[0, i] = tokenIds[i];
			attentionMaskTensor[0, i] = 1;
			tokenTypeIdsTensor[0, i] = 0;
		}

		List<NamedOnnxValue> inputs =
		[
			NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor),
			NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor),
			NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIdsTensor),
		];

		using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = _session.Run(inputs);

		// The model outputs token_embeddings with shape [1, seq_len, 384]
		DisposableNamedOnnxValue tokenEmbeddings = results.First();
		float[] data = tokenEmbeddings.AsEnumerable<float>().ToArray();

		// Mean pooling over all tokens (no padding since EncodeToIds doesn't pad)
		float[] pooled = new float[EmbeddingDimension];

		for (int i = 0; i < seqLen; i++)
		{
			int offset = i * EmbeddingDimension;
			for (int j = 0; j < EmbeddingDimension; j++)
			{
				pooled[j] += data[offset + j];
			}
		}

		for (int j = 0; j < EmbeddingDimension; j++)
		{
			pooled[j] /= seqLen;
		}

		// L2 normalize
		float norm = 0;
		for (int j = 0; j < EmbeddingDimension; j++)
		{
			norm += pooled[j] * pooled[j];
		}

		norm = MathF.Sqrt(norm);
		if (norm > 0)
		{
			for (int j = 0; j < EmbeddingDimension; j++)
			{
				pooled[j] /= norm;
			}
		}

		return pooled;
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_session.Dispose();
			_disposed = true;
		}
	}
}
