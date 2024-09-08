using Application.Queries.Aggregates.TransactionAccounts;
using AutoMapper;
using Domain.Aggregates;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.ViewModels.Aggregates;

namespace API.Controllers.Aggregates;

[ApiController]
[Route("api/[controller]")]
public class TransactionAccountController(IMediator mediator, IMapper mapper, ILogger<TransactionAccountController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for id: {Id}";
	public const string MessageWithoutId = "Error occurred in {Method}";

	private readonly IMediator _mediator = mediator;
	private readonly IMapper _mapper = mapper;
	private readonly ILogger<TransactionAccountController> _logger = logger;

	[HttpGet("by-transaction-id/{transactionId}")]
	public async Task<ActionResult<TransactionAccountVM>> GetTransactionAccountByTransactionId([FromRoute] Guid transactionId)
	{
		try
		{
			_logger.LogDebug("GetTransactionAccountByTransactionId called with transactionId: {transactionId}", transactionId);
			GetTransactionAccountByTransactionIdQuery query = new(transactionId);
			TransactionAccount? result = await _mediator.Send(query);

			if (result == null)
			{
				_logger.LogWarning("GetTransactionAccountByTransactionId called with transactionId: {transactionId} not found", transactionId);
				return NotFound();
			}

			TransactionAccountVM model = _mapper.Map<TransactionAccount, TransactionAccountVM>(result);
			_logger.LogDebug("GetTransactionAccountByTransactionId called with transactionId: {transactionId} found", transactionId);
			return Ok(model);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, MessageWithId, nameof(GetTransactionAccountByTransactionId), transactionId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet("by-receipt-id/{receiptId}")]
	public async Task<ActionResult<List<TransactionAccountVM>>> GetTransactionAccountsByReceiptId([FromRoute] Guid receiptId)
	{
		try
		{
			_logger.LogDebug("GetTransactionAccountsByReceiptId called with receiptId: {receiptId}", receiptId);
			GetTransactionAccountsByReceiptIdQuery query = new(receiptId);
			List<TransactionAccount>? result = await _mediator.Send(query);

			if (result == null)
			{
				_logger.LogWarning("GetTransactionAccountsByReceiptId called with receiptId: {receiptId} not found", receiptId);
				return NotFound();
			}

			List<TransactionAccountVM> model = result.Select(_mapper.Map<TransactionAccount, TransactionAccountVM>).ToList();
			_logger.LogDebug("GetTransactionAccountsByReceiptId called with receiptId: {receiptId} found", receiptId);
			return Ok(model);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, MessageWithId, nameof(GetTransactionAccountsByReceiptId), receiptId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}
}
