using Application.Interfaces.Services;
using Application.Models;
using Application.Models.Ocr;
using Domain.Core;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Resolves VLM-extracted payment tenders to <see cref="ProposedTransaction"/> rows by
/// matching the extracted last-four digits against the user's active cards (matched on
/// <see cref="Card.CardCode"/>). The current schema does not carry a dedicated last-four
/// column, so we treat <see cref="Card.CardCode"/> as the matching key — it is the
/// user-facing card identifier and is commonly populated with the last four digits.
/// Users with a different scheme will simply see no auto-match and fall through to the
/// manual picker, which is the correct degenerate behaviour.
/// </summary>
public class ProposedTransactionResolver(
	ICardService cardService,
	ILogger<ProposedTransactionResolver> logger) : IProposedTransactionResolver
{
	public async Task<List<ProposedTransaction>> ResolveAsync(
		IReadOnlyList<ParsedPayment> payments,
		FieldConfidence<DateOnly> receiptDate,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(payments);

		List<ProposedTransaction> result = new(payments.Count);
		if (payments.Count == 0)
		{
			return result;
		}

		// Materialise the active-card list once. The number of cards is small (single-user
		// scope), so an in-memory linear scan per payment is fine and avoids N round-trips
		// to the repository.
		PagedResult<Card> activeCards = await cardService.GetAllAsync(
			offset: 0,
			limit: int.MaxValue,
			sort: SortParams.Default,
			isActive: true,
			cancellationToken);

		FieldConfidence<DateOnly?> dateForRow = receiptDate.IsPresent
			? new FieldConfidence<DateOnly?>(receiptDate.Value, receiptDate.Confidence)
			: FieldConfidence<DateOnly?>.None();

		foreach (ParsedPayment payment in payments)
		{
			result.Add(ResolveOne(payment, activeCards.Data, dateForRow));
		}

		return result;
	}

	private ProposedTransaction ResolveOne(
		ParsedPayment payment,
		IReadOnlyList<Card> activeCards,
		FieldConfidence<DateOnly?> dateForRow)
	{
		FieldConfidence<decimal?> amount = payment.Amount;
		FieldConfidence<string?> methodSnapshot = payment.Method;

		string? lastFour = payment.LastFour.Value;
		bool hasLastFour = !string.IsNullOrWhiteSpace(lastFour);

		// No last-four → no card resolution possible. CardId/AccountId are absent
		// (None confidence): the wizard pre-fills the Amount/Date/method but leaves
		// the user to pick a card and account manually.
		if (!hasLastFour)
		{
			return new ProposedTransaction(
				CardId: FieldConfidence<Guid?>.None(),
				AccountId: FieldConfidence<Guid?>.None(),
				Amount: amount,
				Date: dateForRow,
				MethodSnapshot: methodSnapshot);
		}

		// Two-pass match (RECEIPTS-667). First try an exact case-insensitive compare so
		// users with bare last-four CardCodes (e.g. "3409") still get the deterministic
		// tie-break they used to. Then fall back to a "trailing four digits" comparison
		// so users whose CardCode embeds the digits in a richer label (e.g. "MC-3409",
		// "Mastercard ****3409", "Chase Visa 3409") still auto-match. The trailing-four
		// extraction is also applied to <paramref name="lastFour"/> in case the VLM
		// emits the four digits with surrounding noise.
		List<Card> matches = [..
			activeCards.Where(c => string.Equals(c.CardCode, lastFour, StringComparison.OrdinalIgnoreCase))];

		if (matches.Count == 0)
		{
			string? lastFourDigits = ExtractTrailingFourDigits(lastFour);
			if (!string.IsNullOrEmpty(lastFourDigits))
			{
				matches = [..
					activeCards.Where(c => ExtractTrailingFourDigits(c.CardCode) == lastFourDigits)];
			}
		}

		if (matches.Count == 1)
		{
			Card card = matches[0];
			return new ProposedTransaction(
				CardId: FieldConfidence<Guid?>.High(card.Id),
				AccountId: FieldConfidence<Guid?>.High(card.AccountId),
				Amount: amount,
				Date: dateForRow,
				MethodSnapshot: methodSnapshot);
		}

		if (matches.Count > 1)
		{
			// Ambiguous match: surface as Low confidence on cardId (and accountId) so the
			// wizard renders a review chip and forces the user to disambiguate. Don't
			// guess — picking the wrong card would silently file a transaction against
			// the wrong account.
			return new ProposedTransaction(
				CardId: FieldConfidence<Guid?>.Low(null),
				AccountId: FieldConfidence<Guid?>.Low(null),
				Amount: amount,
				Date: dateForRow,
				MethodSnapshot: methodSnapshot);
		}

		// No matches: this card last-four is new to the user. None confidence means the
		// chip omits a badge entirely; the wizard simply requires manual selection.
		// Logged at Information so dropped-prefill regressions are diagnosable without
		// emitting card PII (only the four digits, which the user already saw on the
		// receipt photo upload).
		logger.LogInformation(
			"No active Card matched VLM lastFour {LastFour} ({ActiveCardCount} active cards scanned)",
			lastFour, activeCards.Count);
		return new ProposedTransaction(
			CardId: FieldConfidence<Guid?>.None(),
			AccountId: FieldConfidence<Guid?>.None(),
			Amount: amount,
			Date: dateForRow,
			MethodSnapshot: methodSnapshot);
	}

	/// <summary>
	/// Extract the last four digits from <paramref name="raw"/>, skipping non-digit
	/// characters. Returns null when the input has fewer than four digits. Used to
	/// match a VLM-extracted last-four against a richer Card.CardCode label like
	/// "MC-3409" or "Mastercard ****3409". See RECEIPTS-667.
	/// </summary>
	internal static string? ExtractTrailingFourDigits(string? raw)
	{
		if (string.IsNullOrWhiteSpace(raw))
		{
			return null;
		}

		string digits = new([.. raw.Where(char.IsAsciiDigit)]);
		return digits.Length >= 4 ? digits[^4..] : null;
	}
}
