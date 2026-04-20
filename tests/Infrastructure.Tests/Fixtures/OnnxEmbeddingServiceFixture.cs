using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Infrastructure.Tests.Fixtures;

public class OnnxEmbeddingServiceFixture : IDisposable
{
	private bool _disposed;

	public OnnxEmbeddingService Service { get; }

	public OnnxEmbeddingServiceFixture()
	{
		ILogger<OnnxEmbeddingService> logger = new Mock<ILogger<OnnxEmbeddingService>>().Object;
		Service = new OnnxEmbeddingService(logger);
	}

	public static bool ModelExists
	{
		get
		{
			string modelPath = Path.Combine(AppContext.BaseDirectory, "Models", "BgeLargeEnV15", "model.onnx");
			return File.Exists(modelPath);
		}
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			Service.Dispose();
			_disposed = true;
		}

		GC.SuppressFinalize(this);
	}
}
