using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Interfaces.Services;
using Application.Models.Ocr;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public sealed class OllamaReceiptExtractionService : IReceiptExtractionService
{
	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
	{
		NumberHandling = JsonNumberHandling.AllowReadingFromString,
	};

	private readonly HttpClient _httpClient;
	private readonly VlmOcrOptions _options;
	private readonly ILogger<OllamaReceiptExtractionService> _logger;

	public OllamaReceiptExtractionService(
		HttpClient httpClient,
		VlmOcrOptions options,
		ILogger<OllamaReceiptExtractionService> logger)
	{
		_httpClient = httpClient;
		_options = options;
		_logger = logger;
	}

	public async Task<ParsedReceipt> ExtractAsync(byte[] imageBytes, string contentType, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(imageBytes);
		if (imageBytes.Length == 0)
		{
			throw new ArgumentException("Image bytes cannot be empty.", nameof(imageBytes));
		}

		_logger.LogDebug(
			"Extracting receipt via Ollama VLM (model={Model}, bytes={Bytes}, contentType={ContentType})",
			_options.Model, imageBytes.Length, contentType);

		string base64 = Convert.ToBase64String(imageBytes);
		OllamaGenerateRequest request = new(
			Model: _options.Model,
			Prompt: ReceiptExtractionPrompt.Current,
			Images: [base64]);

		using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		timeoutSource.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

		HttpResponseMessage httpResponse;
		try
		{
			httpResponse = await _httpClient.PostAsJsonAsync("api/generate", request, JsonOptions, timeoutSource.Token);
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
		{
			throw new TimeoutException($"Ollama VLM call timed out after {_options.TimeoutSeconds}s.");
		}

		httpResponse.EnsureSuccessStatusCode();

		OllamaGenerateResponse? generateResponse;
		try
		{
			generateResponse = await httpResponse.Content.ReadFromJsonAsync<OllamaGenerateResponse>(JsonOptions, timeoutSource.Token);
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
		{
			throw new TimeoutException($"Ollama VLM response read timed out after {_options.TimeoutSeconds}s.");
		}

		if (generateResponse is null || string.IsNullOrWhiteSpace(generateResponse.Response))
		{
			throw new InvalidOperationException("Ollama VLM returned an empty response.");
		}

		_logger.LogDebug("Ollama VLM raw response: {Response}", generateResponse.Response);

		VlmReceiptPayload payload;
		try
		{
			payload = JsonSerializer.Deserialize<VlmReceiptPayload>(generateResponse.Response, JsonOptions)
				?? throw new InvalidOperationException("Ollama VLM produced a null receipt payload.");
		}
		catch (JsonException ex)
		{
			throw new InvalidOperationException(
				$"Failed to parse Ollama VLM response as JSON. Raw response: {generateResponse.Response}", ex);
		}

		return MapToParsedReceipt(payload);
	}

	private static ParsedReceipt MapToParsedReceipt(VlmReceiptPayload payload)
	{
		FieldConfidence<string> storeName = !string.IsNullOrWhiteSpace(payload.Store)
			? FieldConfidence<string>.High(payload.Store)
			: FieldConfidence<string>.None();

		FieldConfidence<DateOnly> date = payload.Date is { } d
			? FieldConfidence<DateOnly>.High(d)
			: FieldConfidence<DateOnly>.None();

		List<ParsedReceiptItem> items = (payload.Items ?? []).Select(MapItem).ToList();

		FieldConfidence<decimal> subtotal = payload.Subtotal is { } s
			? FieldConfidence<decimal>.High(s)
			: FieldConfidence<decimal>.None();

		List<ParsedTaxLine> taxLines = (payload.TaxLines ?? []).Select(MapTaxLine).ToList();

		FieldConfidence<decimal> total = payload.Total is { } t
			? FieldConfidence<decimal>.High(t)
			: FieldConfidence<decimal>.None();

		FieldConfidence<string?> paymentMethod = !string.IsNullOrWhiteSpace(payload.PaymentMethod)
			? FieldConfidence<string?>.High(payload.PaymentMethod)
			: FieldConfidence<string?>.None();

		return new ParsedReceipt(storeName, date, items, subtotal, taxLines, total, paymentMethod);
	}

	private static ParsedReceiptItem MapItem(VlmReceiptItem item)
	{
		FieldConfidence<string?> code = !string.IsNullOrWhiteSpace(item.Code)
			? FieldConfidence<string?>.High(item.Code)
			: FieldConfidence<string?>.None();

		FieldConfidence<string> description = !string.IsNullOrWhiteSpace(item.Description)
			? FieldConfidence<string>.High(item.Description)
			: FieldConfidence<string>.None();

		FieldConfidence<decimal> quantity = item.Quantity is { } q
			? FieldConfidence<decimal>.High(q)
			: FieldConfidence<decimal>.None();

		FieldConfidence<decimal> unitPrice = item.UnitPrice is { } u
			? FieldConfidence<decimal>.High(u)
			: FieldConfidence<decimal>.None();

		FieldConfidence<decimal> totalPrice = item.TotalPrice is { } t
			? FieldConfidence<decimal>.High(t)
			: FieldConfidence<decimal>.None();

		return new ParsedReceiptItem(code, description, quantity, unitPrice, totalPrice);
	}

	private static ParsedTaxLine MapTaxLine(VlmTaxLine tax)
	{
		FieldConfidence<string> label = !string.IsNullOrWhiteSpace(tax.Label)
			? FieldConfidence<string>.High(tax.Label)
			: FieldConfidence<string>.None();

		FieldConfidence<decimal> amount = tax.Amount is { } a
			? FieldConfidence<decimal>.High(a)
			: FieldConfidence<decimal>.None();

		return new ParsedTaxLine(label, amount);
	}
}
