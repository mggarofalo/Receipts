using System.ComponentModel.DataAnnotations;

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

	/// <summary>
	/// Default Ollama base URL when no other configuration source is wired up. The production
	/// path overrides this via <c>Ocr:Vlm:OllamaUrl</c> or the Aspire-injected
	/// <c>Ollama:BaseUrl</c>; see <c>InfrastructureService.ResolveOllamaUrl</c>. The default
	/// keeps local <c>dotnet run</c> sessions (no Aspire, no env vars) talking to a developer's
	/// localhost daemon instead of crashing at startup.
	/// </summary>
	public const string DefaultOllamaUrl = "http://localhost:11434";

	/// <summary>
	/// Default upper bound on the raw image byte length accepted by the VLM extraction service.
	/// Base64 inflates the request body by ~33%, and Ollama's default request body limit is
	/// well below the kind of camera dumps mobile clients can produce (50 MB+ on modern phones).
	/// 15 MB is generous for receipt photos at typical rasterization resolutions while keeping
	/// the post-base64 body comfortably under Ollama's threshold. See RECEIPTS-640.
	/// </summary>
	public const int DefaultMaxImageBytes = 15 * 1024 * 1024;

	/// <summary>
	/// Base URL of the Ollama instance hosting the VLM. Bound from <c>Ocr:Vlm:OllamaUrl</c>
	/// with a <c>PostConfigure</c> fallback to <c>Ollama:BaseUrl</c> (Aspire-injected) when
	/// the explicit override is absent. <see cref="DefaultOllamaUrl"/> keeps non-Aspire
	/// <c>dotnet run</c> sessions working out of the box; the property is non-nullable so
	/// downstream code can dereference it without null checks. The DataAnnotations
	/// <see cref="RequiredAttribute"/> guards against a future refactor that erases the
	/// default and lets an empty string slip through binding.
	/// </summary>
	[Required(AllowEmptyStrings = false)]
	public string OllamaUrl { get; set; } = DefaultOllamaUrl;

	/// <summary>
	/// Model tag passed to Ollama's <c>/api/generate</c> endpoint. Defaults to
	/// <see cref="DefaultModel"/>; override via <c>Ocr:Vlm:Model</c> when running tests
	/// against alternative models (e.g. qwen2.5vl).
	/// </summary>
	[Required(AllowEmptyStrings = false)]
	public string Model { get; set; } = DefaultModel;

	/// <summary>
	/// Per-attempt timeout for the VLM call in seconds. Each retry receives a fresh budget
	/// (the resilience pipeline composes Retry around Timeout — see RECEIPTS-630). Range
	/// is 1..3600 to keep operators from accidentally configuring an infinite or zero
	/// timeout via mis-typed config.
	/// </summary>
	[Range(1, 3600)]
	public int TimeoutSeconds { get; set; } = 120;

	/// <summary>
	/// Maximum image byte length accepted by <c>OllamaReceiptExtractionService.ExtractAsync</c>.
	/// Inputs larger than this throw <see cref="ArgumentException"/> before any base64 encoding
	/// happens, protecting both the client (memory) and the Ollama server (request body limit).
	/// See RECEIPTS-640.
	/// </summary>
	[Range(1, int.MaxValue)]
	public int MaxImageBytes { get; set; } = DefaultMaxImageBytes;

	/// <summary>
	/// When <c>true</c>, the raw VLM response body is logged at <c>Debug</c> level after each
	/// successful call. The raw body contains receipt PII — store name, item descriptions,
	/// payment method, and last-four card digits — so this flag MUST stay <c>false</c> in
	/// production. It exists for local diagnostics and the <c>VlmEval</c> tool only. See
	/// RECEIPTS-639.
	/// </summary>
	public bool LogRawResponses { get; set; }
}
