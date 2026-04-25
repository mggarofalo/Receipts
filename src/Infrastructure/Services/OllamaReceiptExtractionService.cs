using System.Globalization;
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

		OllamaGenerateResponse? generateResponse;
		try
		{
			using HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync(
				"api/generate", request, JsonOptions, timeoutSource.Token);

			httpResponse.EnsureSuccessStatusCode();

			generateResponse = await httpResponse.Content.ReadFromJsonAsync<OllamaGenerateResponse>(
				JsonOptions, timeoutSource.Token);
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
		{
			throw new TimeoutException($"Ollama VLM call timed out after {_options.TimeoutSeconds}s.");
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
		FieldConfidence<string> storeName = !string.IsNullOrWhiteSpace(payload.Store?.Name)
			? FieldConfidence<string>.High(payload.Store.Name)
			: FieldConfidence<string>.None();

		FieldConfidence<DateOnly> date = TryParseDate(payload.Datetime) is { } d
			? FieldConfidence<DateOnly>.High(d)
			: FieldConfidence<DateOnly>.None();

		List<ParsedReceiptItem> items = MergeWeightSublines(payload.Items ?? []).Select(MapItem).ToList();

		FieldConfidence<decimal> subtotal = payload.Subtotal is { } s
			? FieldConfidence<decimal>.High(s)
			: FieldConfidence<decimal>.None();

		List<ParsedTaxLine> taxLines = (payload.TaxLines ?? []).Select(MapTaxLine).ToList();

		FieldConfidence<decimal> total = payload.Total is { } t
			? FieldConfidence<decimal>.High(t)
			: FieldConfidence<decimal>.None();

		// Collapse first payment's method into the single-string PaymentMethod on ParsedReceipt.
		// Multi-tender receipts lose detail here; see RECEIPTS-626 for the domain-model extension.
		string? primaryMethod = payload.Payments?
			.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Method))?.Method;
		FieldConfidence<string?> paymentMethod = !string.IsNullOrWhiteSpace(primaryMethod)
			? FieldConfidence<string?>.High(primaryMethod)
			: FieldConfidence<string?>.None();

		return new ParsedReceipt(storeName, date, items, subtotal, taxLines, total, paymentMethod);
	}

	/// <summary>
	/// Merges weighted-item sub-lines into their parent item. VLMs (both qwen2.5vl:3b and :7b)
	/// emit "2.460 lb. @ 1 lb. /0.50" as its own item row when prompted via few-shot examples
	/// (they reliably extract the quantity/unitPrice but not the parent-merging structure).
	/// We detect these sub-lines deterministically: null <c>code</c> + description containing
	/// <c>" @ "</c> + non-null <c>quantity</c> and <c>unitPrice</c>. When the preceding item
	/// shares the same <c>lineTotal</c> and has null <c>quantity</c>, we absorb the sub-line's
	/// quantity/unitPrice into the parent and drop the sub-line.
	/// </summary>
	internal static List<VlmReceiptItem> MergeWeightSublines(List<VlmReceiptItem> items)
	{
		List<VlmReceiptItem> merged = [];
		foreach (VlmReceiptItem item in items)
		{
			if (IsWeightSubline(item) && merged.Count > 0)
			{
				VlmReceiptItem parent = merged[^1];
				if (parent.LineTotal == item.LineTotal && parent.Quantity is null)
				{
					parent.Quantity = item.Quantity;
					parent.UnitPrice = item.UnitPrice;
					continue;
				}
			}
			merged.Add(item);
		}
		return merged;
	}

	private static bool IsWeightSubline(VlmReceiptItem item)
	{
		return string.IsNullOrWhiteSpace(item.Code)
			&& !string.IsNullOrWhiteSpace(item.Description)
			&& item.Description.Contains(" @ ", StringComparison.Ordinal)
			&& item.Quantity is not null
			&& item.UnitPrice is not null;
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

		FieldConfidence<decimal> totalPrice = item.LineTotal is { } t
			? FieldConfidence<decimal>.High(t)
			: FieldConfidence<decimal>.None();

		return new ParsedReceiptItem(code, description, quantity, unitPrice, totalPrice);
	}

	private static readonly string[] DateFormats =
	[
		"yyyy-MM-dd",
		"yyyy/MM/dd",
		"MM/dd/yyyy",
		"M/d/yyyy",
		"MM/dd/yy",
		"M/d/yy",
		"dd/MM/yyyy",
		"d/M/yyyy",
		"dd-MM-yyyy",
		"dd.MM.yyyy",
	];

	private static DateOnly? TryParseDate(string? raw)
	{
		if (string.IsNullOrWhiteSpace(raw))
		{
			return null;
		}

		string trimmed = raw.Trim();

		// VLM may return date + time separated by 'T' (ISO-8601) or ' '
		// (e.g. "2026-01-14T17:57:20" or "01/14/26 17:57:20"). Keep the date part only.
		int splitIndex = trimmed.IndexOfAny(['T', ' ']);
		if (splitIndex > 0)
		{
			trimmed = trimmed[..splitIndex];
		}

		if (DateOnly.TryParseExact(
			trimmed, DateFormats, CultureInfo.InvariantCulture,
			DateTimeStyles.None, out DateOnly exact))
		{
			return exact;
		}

		return DateOnly.TryParse(
			trimmed, CultureInfo.InvariantCulture,
			DateTimeStyles.None, out DateOnly invariant)
			? invariant
			: null;
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
