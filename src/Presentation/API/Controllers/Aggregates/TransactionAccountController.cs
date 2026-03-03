using API.Generated.Dtos;
using API.Mapping.Aggregates;
using Application.Queries.Aggregates.TransactionAccounts;
using Asp.Versioning;
using Domain.Aggregates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Aggregates;

[ApiVersion("1.0")]
[ApiController]
[Route("api/transaction-accounts")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class TransactionAccountController(IMediator mediator, TransactionAccountMapper mapper, ILogger<TransactionAccountController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for id: {Id}";
	public const string MessageWithoutId = "Error occurred in {Method}";

	[HttpGet("")]
	[EndpointSummary("Get transaction accounts")]
	[EndpointDescription("Returns transaction accounts filtered by transaction ID or receipt ID.")]
	[ProducesResponseType<List<TransactionAccountResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> GetTransactionAccounts([FromQuery] Guid? transactionId = null, [FromQuery] Guid? receiptId = null)
	{
		try
		{
			if (transactionId.HasValue && receiptId.HasValue)
			{
				return BadRequest("Provide either transactionId or receiptId, not both.");
			}

			if (transactionId.HasValue)
			{
				logger.LogDebug("GetTransactionAccounts called with transactionId: {transactionId}", transactionId.Value);
				GetTransactionAccountByTransactionIdQuery query = new(transactionId.Value);
				TransactionAccount? result = await mediator.Send(query);

				if (result == null)
				{
					return NotFound();
				}

				TransactionAccountResponse model = mapper.ToResponse(result);
				return Ok(new List<TransactionAccountResponse> { model });
			}

			if (receiptId.HasValue)
			{
				logger.LogDebug("GetTransactionAccounts called with receiptId: {receiptId}", receiptId.Value);
				GetTransactionAccountsByReceiptIdQuery query = new(receiptId.Value);
				List<TransactionAccount>? result = await mediator.Send(query);

				if (result == null)
				{
					return NotFound();
				}

				List<TransactionAccountResponse> model = [.. result.Select(mapper.ToResponse)];
				return Ok(model);
			}

			return BadRequest("Either transactionId or receiptId query parameter is required.");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetTransactionAccounts));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}
}
