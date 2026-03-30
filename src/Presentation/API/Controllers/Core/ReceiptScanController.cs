using API.Generated.Dtos;
using Application.Commands.Receipt.Scan;
using Application.Exceptions;
using Application.Models.Ocr;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using DtoConfidenceLevel = API.Generated.Dtos.ConfidenceLevel;
using OcrConfidenceLevel = Application.Models.Ocr.ConfidenceLevel;

namespace API.Controllers.Core;

[ApiVersion("1.0")]
[ApiController]
[Route("api/receipts")]
[Authorize]
public class ReceiptScanController(
	IMediator mediator,
	ILogger<ReceiptScanController> logger) : ControllerBase
{
	private const long MaxFileSizeBytes = 20 * 1024 * 1024; // 20 MB

	private static readonly HashSet<string> AllowedContentTypes =
	[
		"image/jpeg",
		"image/png",
	];

	[HttpPost("scan")]
	[RequestSizeLimit(20 * 1024 * 1024)]
	[EndpointSummary("Scan a receipt image and return a proposed receipt")]
	[EndpointDescription("Accepts a JPEG or PNG image, runs preprocessing, OCR, and parsing, then returns a proposed receipt with per-field confidence scores. The proposal is NOT persisted.")]
	[ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
	public async Task<Results<Ok<ProposedReceiptResponse>, BadRequest<string>, StatusCodeHttpResult, UnprocessableEntity<string>>> ScanReceipt(
		IFormFile? file)
	{
		if (file is null || file.Length == 0)
		{
			return TypedResults.BadRequest("No file was uploaded.");
		}

		if (file.Length > MaxFileSizeBytes)
		{
			return TypedResults.BadRequest($"File size exceeds the maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)} MB.");
		}

		if (!AllowedContentTypes.Contains(file.ContentType))
		{
			return TypedResults.StatusCode(StatusCodes.Status415UnsupportedMediaType);
		}

		byte[] imageBytes;
		using (MemoryStream ms = new())
		{
			await file.CopyToAsync(ms);
			imageBytes = ms.ToArray();
		}

		ScanReceiptCommand command;
		try
		{
			command = new ScanReceiptCommand(imageBytes, file.ContentType);
		}
		catch (ArgumentException ex)
		{
			return TypedResults.BadRequest(ex.Message);
		}

		ScanReceiptResult result;
		try
		{
			result = await mediator.Send(command);
		}
		catch (OcrNoTextException ex)
		{
			logger.LogWarning(ex, "OCR returned no text for scanned receipt image");
			return TypedResults.UnprocessableEntity("The image could not be read or OCR returned no text.");
		}
		catch (InvalidOperationException ex)
		{
			logger.LogWarning(ex, "Failed to process scanned receipt image");
			return TypedResults.UnprocessableEntity(ex.Message);
		}

		return TypedResults.Ok(MapToResponse(result));
	}

	private static ProposedReceiptResponse MapToResponse(ScanReceiptResult result)
	{
		ParsedReceipt parsed = result.ParsedReceipt;

		return new ProposedReceiptResponse
		{
			StoreName = parsed.StoreName.Value,
			StoreNameConfidence = MapConfidence(parsed.StoreName.Confidence),
			Date = parsed.Date.Value,
			DateConfidence = MapConfidence(parsed.Date.Confidence),
			Items = parsed.Items.Select(MapItem).ToList(),
			Subtotal = ToNullableDouble(parsed.Subtotal.Value),
			SubtotalConfidence = MapConfidence(parsed.Subtotal.Confidence),
			TaxLines = parsed.TaxLines.Select(MapTaxLine).ToList(),
			Total = ToNullableDouble(parsed.Total.Value),
			TotalConfidence = MapConfidence(parsed.Total.Confidence),
			PaymentMethod = parsed.PaymentMethod.Value,
			PaymentMethodConfidence = MapConfidence(parsed.PaymentMethod.Confidence),
			RawOcrText = result.RawOcrText,
			OcrConfidence = result.OcrConfidence,
		};
	}

	private static ProposedReceiptItemResponse MapItem(ParsedReceiptItem item)
	{
		return new ProposedReceiptItemResponse
		{
			Code = item.Code.Value,
			CodeConfidence = MapConfidence(item.Code.Confidence),
			Description = item.Description.Value,
			DescriptionConfidence = MapConfidence(item.Description.Confidence),
			Quantity = ToNullableDouble(item.Quantity.Value),
			QuantityConfidence = MapConfidence(item.Quantity.Confidence),
			UnitPrice = ToNullableDouble(item.UnitPrice.Value),
			UnitPriceConfidence = MapConfidence(item.UnitPrice.Confidence),
			TotalPrice = ToNullableDouble(item.TotalPrice.Value),
			TotalPriceConfidence = MapConfidence(item.TotalPrice.Confidence),
		};
	}

	private static ProposedTaxLineResponse MapTaxLine(ParsedTaxLine taxLine)
	{
		return new ProposedTaxLineResponse
		{
			Label = taxLine.Label.Value,
			LabelConfidence = MapConfidence(taxLine.Label.Confidence),
			Amount = ToNullableDouble(taxLine.Amount.Value),
			AmountConfidence = MapConfidence(taxLine.Amount.Confidence),
		};
	}

	private static double? ToNullableDouble(decimal? value) =>
		value.HasValue ? (double)value.Value : null;

	private static DtoConfidenceLevel MapConfidence(OcrConfidenceLevel level)
	{
		return level switch
		{
			OcrConfidenceLevel.Low => DtoConfidenceLevel.Low,
			OcrConfidenceLevel.Medium => DtoConfidenceLevel.Medium,
			OcrConfidenceLevel.High => DtoConfidenceLevel.High,
			_ => DtoConfidenceLevel.Low,
		};
	}
}
