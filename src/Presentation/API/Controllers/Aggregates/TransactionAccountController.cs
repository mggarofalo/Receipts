using API.Mapping.Aggregates;
using Application.Queries.Aggregates.TransactionAccounts;
using Domain.Aggregates;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.ViewModels.Aggregates;

namespace API.Controllers.Aggregates;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TransactionAccountController(IMediator mediator, TransactionAccountMapper mapper, ILogger<TransactionAccountController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for id: {Id}";
	public const string MessageWithoutId = "Error occurred in {Method}";
	public const string RouteByTransactionId = "by-transaction-id/{transactionId}";
	public const string RouteByReceiptId = "by-receipt-id/{receiptId}";

	/// <summary>
	/// Get a transaction account by transaction ID
	/// </summary>
	/// <param name="transactionId">The ID of the transaction</param>
	/// <returns>The transaction account</returns>
	/// <response code="200">Returns the transaction account</response>
	/// <response code="404">If the transaction account is not found</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpGet(RouteByTransactionId)]
	[ProducesResponseType(typeof(TransactionAccountVM), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<TransactionAccountVM>> GetTransactionAccountByTransactionId([FromRoute] Guid transactionId)
	{
		try
		{
			logger.LogDebug("GetTransactionAccountByTransactionId called with transactionId: {transactionId}", transactionId);
			GetTransactionAccountByTransactionIdQuery query = new(transactionId);
			TransactionAccount? result = await mediator.Send(query);

			if (result == null)
			{
				logger.LogWarning("GetTransactionAccountByTransactionId called with transactionId: {transactionId} not found", transactionId);
				return NotFound();
			}

			TransactionAccountVM model = mapper.ToViewModel(result);
			logger.LogDebug("GetTransactionAccountByTransactionId called with transactionId: {transactionId} found", transactionId);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetTransactionAccountByTransactionId), transactionId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	/// <summary>
	/// Get transaction accounts by receipt ID
	/// </summary>
	/// <param name="receiptId">The ID of the receipt</param>
	/// <returns>A list of transaction accounts associated with the receipt</returns>
	/// <response code="200">Returns the list of transaction accounts</response>
	/// <response code="404">If no transaction accounts are found for the receipt</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpGet(RouteByReceiptId)]
	[ProducesResponseType(typeof(List<TransactionAccountVM>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<TransactionAccountVM>>> GetTransactionAccountsByReceiptId([FromRoute] Guid receiptId)
	{
		try
		{
			logger.LogDebug("GetTransactionAccountsByReceiptId called with receiptId: {receiptId}", receiptId);
			GetTransactionAccountsByReceiptIdQuery query = new(receiptId);
			List<TransactionAccount>? result = await mediator.Send(query);

			if (result == null)
			{
				logger.LogWarning("GetTransactionAccountsByReceiptId called with receiptId: {receiptId} not found", receiptId);
				return NotFound();
			}

			List<TransactionAccountVM> model = result.Select(mapper.ToViewModel).ToList();
			logger.LogDebug("GetTransactionAccountsByReceiptId called with receiptId: {receiptId} found", receiptId);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetTransactionAccountsByReceiptId), receiptId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}
}
