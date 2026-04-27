namespace Infrastructure.Services;

public sealed class VlmOcrOptions
{
	/// <summary>
	/// Single source of truth for the VLM model tag used by Ollama. Referenced by C# defaults
	/// (this options class) and Aspire/docker-compose via the <c>VLM_MODEL</c> env var. Changing
	/// the model name should mean editing this constant or setting <c>VLM_MODEL</c> — never
	/// touching individual files (RECEIPTS-635).
	/// </summary>
	public const string DefaultModel = "glm-ocr:q8_0";

	public string? OllamaUrl { get; set; }

	public string Model { get; set; } = DefaultModel;

	public int TimeoutSeconds { get; set; } = 120;

	/// <summary>
	/// When <c>true</c>, the raw VLM response body is logged at <c>Debug</c> level after each
	/// successful call. The raw body contains receipt PII — store name, item descriptions,
	/// payment method, and last-four card digits — so this flag MUST stay <c>false</c> in
	/// production. It exists for local diagnostics and the <c>VlmEval</c> tool only. See
	/// RECEIPTS-639.
	/// </summary>
	public bool LogRawResponses { get; set; }
}
