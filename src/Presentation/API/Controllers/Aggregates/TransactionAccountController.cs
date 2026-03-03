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
public class TransactionAccountController(IMediator mediator, TransactionAccountMapper mapper) : ControllerBase
{
	[HttpGet("")]
	[EndpointSummary("Get transaction accounts")]
	[EndpointDescription("Returns transaction accounts filtered by transaction ID or receipt ID.")]
	[ProducesResponseType<List<TransactionAccountResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> GetTransactionAccounts([FromQuery] Guid? transactionId = null, [FromQuery] Guid? receiptId = null)
	{
		if (transactionId.HasValue && receiptId.HasValue)
		{
			return BadRequest("Provide either transactionId or receiptId, not both.");
		}

		if (transactionId.HasValue)
		{
			GetTransactionAccountByTransactionIdQuery query = new(transactionId.Value);
			TransactionAccount? result = await mediator.Send(query);

			List<TransactionAccountResponse> model = result != null
				? [mapper.ToResponse(result)]
				: [];
			return Ok(model);
		}

		if (receiptId.HasValue)
		{
			GetTransactionAccountsByReceiptIdQuery query = new(receiptId.Value);
			List<TransactionAccount>? result = await mediator.Send(query);

			List<TransactionAccountResponse> model = [.. (result ?? []).Select(mapper.ToResponse)];
			return Ok(model);
		}

		return BadRequest("Either transactionId or receiptId query parameter is required.");
	}
}
