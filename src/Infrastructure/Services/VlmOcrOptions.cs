namespace Infrastructure.Services;

public sealed class VlmOcrOptions
{
	public string? OllamaUrl { get; set; }

	public string Model { get; set; } = "glm-ocr:q8_0";

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
