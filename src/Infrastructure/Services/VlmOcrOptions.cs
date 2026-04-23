namespace Infrastructure.Services;

public sealed class VlmOcrOptions
{
	public string? OllamaUrl { get; set; }

	public string Model { get; set; } = "glm-ocr:q8_0";

	public int TimeoutSeconds { get; set; } = 120;
}
