using API.Generated.Dtos;
using API.Mapping.Aggregates;
using Application.Queries.Aggregates.TransactionAccounts;
using Domain.Aggregates;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Aggregates;

[ApiController]
[Route("api/transaction-accounts")]
[Produces("application/json")]
public class TransactionAccountController(IMediator mediator, TransactionAccountMapper mapper, ILogger<TransactionAccountController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for id: {Id}";
	public const string MessageWithoutId = "Error occurred in {Method}";
	public const string RouteByTransactionId = "by-transaction-id/{transactionId}";
	public const string RouteByReceiptId = "by-receipt-id/{receiptId}";

	[HttpGet(RouteByTransactionId)]
	[EndpointSummary("Get a transaction with its account")]
	[EndpointDescription("Returns a transaction joined with its associated account, looked up by transaction ID.")]
	[ProducesResponseType<TransactionAccountResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<TransactionAccountResponse>> GetTransactionAccountByTransactionId([FromRoute] Guid transactionId)
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

			TransactionAccountResponse model = mapper.ToResponse(result);
			logger.LogDebug("GetTransactionAccountByTransactionId called with transactionId: {transactionId} found", transactionId);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetTransactionAccountByTransactionId), transactionId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet(RouteByReceiptId)]
	[EndpointSummary("Get transaction accounts by receipt ID")]
	[EndpointDescription("Returns all transactions joined with their associated accounts for the specified receipt.")]
	[ProducesResponseType<List<TransactionAccountResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<TransactionAccountResponse>>> GetTransactionAccountsByReceiptId([FromRoute] Guid receiptId)
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

			List<TransactionAccountResponse> model = result.Select(mapper.ToResponse).ToList();
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
