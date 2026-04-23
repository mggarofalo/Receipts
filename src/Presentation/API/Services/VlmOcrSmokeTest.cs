namespace API.Services;

/// <summary>
/// Startup log-only check that the configured Ollama instance is reachable and has
/// the glm-ocr model loaded. Used by <c>Program.cs</c> to surface VLM provisioning
/// problems early without blocking API startup (RECEIPTS-616 epic).
/// </summary>
public static class VlmOcrSmokeTest
{
	private const string ExpectedModel = "glm-ocr";
	private const string TagsEndpoint = "/api/tags";

	public static async Task RunAsync(HttpClient http, ILogger logger, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(http);
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

			string body = await response.Content.ReadAsStringAsync(cancellationToken);
			if (body.Contains(ExpectedModel, StringComparison.OrdinalIgnoreCase))
			{
				logger.LogInformation("VLM OCR: glm-ocr model available at {Url}", http.BaseAddress);
			}
			else
			{
				logger.LogWarning(
					"VLM OCR: reachable at {Url} but glm-ocr not in model list — run \"ollama pull glm-ocr:q8_0\" on the VLM host",
					http.BaseAddress);
			}
		}
		catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
		{
			logger.LogWarning(ex, "VLM OCR: failed to reach {Url}", http.BaseAddress);
		}
	}
}
