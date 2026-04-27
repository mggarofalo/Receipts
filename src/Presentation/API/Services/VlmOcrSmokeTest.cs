using System.Text.Json;
using System.Text.Json.Serialization;
using Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace API.Services;

/// <summary>
/// Startup log-only check that the configured Ollama instance is reachable and has
/// the configured VLM model loaded. Used by <c>Program.cs</c> to surface VLM provisioning
/// problems early without blocking API startup (RECEIPTS-616 epic).
/// <para>
/// The expected model name is sourced from <see cref="VlmOcrOptions.Model"/> (single source of
/// truth — RECEIPTS-635). The smoke test parses Ollama's <c>/api/tags</c> response and matches
/// the configured tag exactly against entries in <c>models[].name</c> — substring matching is
/// avoided so e.g. <c>glm-ocr-experimental</c> never satisfies a check for <c>glm-ocr:q8_0</c>.
/// </para>
/// </summary>
public sealed class VlmOcrSmokeTest
{
	private const string TagsEndpoint = "/api/tags";
	internal const string HttpClientName = "vlm-smoke";

	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

	private readonly IHttpClientFactory _httpClientFactory;
	private readonly VlmOcrOptions _options;
	private readonly ILogger<VlmOcrSmokeTest> _logger;

	public VlmOcrSmokeTest(
		IHttpClientFactory httpClientFactory,
		IOptions<VlmOcrOptions> options,
		ILogger<VlmOcrSmokeTest> logger)
	{
		ArgumentNullException.ThrowIfNull(httpClientFactory);
		ArgumentNullException.ThrowIfNull(options);
		ArgumentNullException.ThrowIfNull(logger);

		_httpClientFactory = httpClientFactory;
		_options = options.Value;
		_logger = logger;
	}

	public Task RunAsync(CancellationToken cancellationToken)
	{
		HttpClient http = _httpClientFactory.CreateClient(HttpClientName);
		return RunAsync(http, _options.Model, _logger, cancellationToken);
	}

	/// <summary>
	/// Probes the supplied <paramref name="http"/> client (already configured with a base address
	/// and timeout) for the configured Ollama tags endpoint and verifies that
	/// <paramref name="expectedModel"/> appears verbatim in the <c>models[].name</c> list. Logs
	/// at Information on success and Warning on any reachability/parse/model-missing condition.
	/// All exceptions are swallowed: the smoke test must never crash startup (RECEIPTS-616).
	/// </summary>
	internal static async Task RunAsync(
		HttpClient http,
		string expectedModel,
		ILogger logger,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(http);
		ArgumentException.ThrowIfNullOrWhiteSpace(expectedModel);
		ArgumentNullException.ThrowIfNull(logger);

		try
		{
			using HttpResponseMessage response = await http.GetAsync(TagsEndpoint, cancellationToken);
			if (!response.IsSuccessStatusCode)
			{
				logger.LogWarning(
					"VLM OCR: {Endpoint} returned {StatusCode} from {Url}",
					TagsEndpoint, (int)response.StatusCode, http.BaseAddress);
				return;
			}

			TagsResponse? tags;
			try
			{
				tags = await response.Content.ReadFromJsonAsync<TagsResponse>(JsonOptions, cancellationToken);
			}
			catch (JsonException ex)
			{
				logger.LogWarning(
					ex,
					"VLM OCR: failed to parse {Endpoint} JSON from {Url}",
					TagsEndpoint, http.BaseAddress);
				return;
			}

			if (tags?.Models is null || tags.Models.Count == 0)
			{
				logger.LogWarning(
					"VLM OCR: reachable at {Url} but no models reported — run \"ollama pull {Model}\" on the VLM host",
					http.BaseAddress, expectedModel);
				return;
			}

			bool present = tags.Models.Any(m =>
				string.Equals(m.Name, expectedModel, StringComparison.Ordinal));

			if (present)
			{
				logger.LogInformation(
					"VLM OCR: {Model} available at {Url}",
					expectedModel, http.BaseAddress);
			}
			else
			{
				logger.LogWarning(
					"VLM OCR: reachable at {Url} but {Model} not in model list — run \"ollama pull {Model}\" on the VLM host",
					http.BaseAddress, expectedModel, expectedModel);
			}
		}
		catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
		{
			logger.LogWarning(ex, "VLM OCR: failed to reach {Url}", http.BaseAddress);
		}
	}

	private sealed record TagsResponse(
		[property: JsonPropertyName("models")] IReadOnlyList<TagsModel>? Models);

	private sealed record TagsModel(
		[property: JsonPropertyName("name")] string? Name);
}
