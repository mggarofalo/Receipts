using API.Controllers.Core;
using API.Generated.Dtos;
using API.Mapping.Core;
using API.Services;
using Application.Commands.Card.Create;
using Application.Commands.Card.Delete;
using Application.Commands.Card.Merge;
using Application.Commands.Card.Update;
using Application.Interfaces.Services;
using Application.Models;
using Application.Models.Merge;
using Application.Queries.Core.Card;
using Domain.Core;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SampleData.Domain.Core;
using SampleData.Dtos.Core;

namespace Presentation.API.Tests.Controllers.Core;

public class CardsControllerTests
{
	private readonly CardMapper _mapper;
	private readonly Mock<IMediator> _mediatorMock;
	private readonly Mock<ILogger<CardsController>> _loggerMock;
	private readonly Mock<IEntityChangeNotifier> _notifierMock;
	private readonly Mock<ICardService> _accountServiceMock;
	private readonly CardsController _controller;

	public CardsControllerTests()
	{
		_mediatorMock = new Mock<IMediator>();
		_mapper = new CardMapper();
		_loggerMock = ControllerTestHelpers.GetLoggerMock<CardsController>();
		_notifierMock = new Mock<IEntityChangeNotifier>();
		_accountServiceMock = new Mock<ICardService>();
		_controller = new CardsController(_mediatorMock.Object, _mapper, _loggerMock.Object, _notifierMock.Object, _accountServiceMock.Object);
		_controller.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext()
		};
	}

	[Fact]
	public async Task GetAccountById_ReturnsOkResult_WhenAccountExists()
	{
		// Arrange
		Card account = CardGenerator.Generate();
		CardResponse expectedReturn = _mapper.ToResponse(account);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetCardByIdQuery>(q => q.Id == account.Id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(account);

		// Act
		Results<Ok<CardResponse>, NotFound> result = await _controller.GetCardById(account.Id);

		// Assert
		Ok<CardResponse> okResult = Assert.IsType<Ok<CardResponse>>(result.Result);
		CardResponse actualReturn = Assert.IsType<CardResponse>(okResult.Value);
		actualReturn.Should().BeEquivalentTo(expectedReturn);
	}

	[Fact]
	public async Task GetAccountById_ReturnsNotFound_WhenAccountDoesNotExist()
	{
		// Arrange
		Guid missingAccountId = Guid.NewGuid();

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetCardByIdQuery>(q => q.Id == missingAccountId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync((Card?)null);

		// Act
		Results<Ok<CardResponse>, NotFound> result = await _controller.GetCardById(missingAccountId);

		// Assert
		Assert.IsType<NotFound>(result.Result);
	}

	[Fact]
	public async Task GetAccountById_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		Guid id = CardGenerator.Generate().Id;

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetCardByIdQuery>(q => q.Id == id),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.GetCardById(id);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task GetAllAccounts_ReturnsOkResult_WithListOfAccounts()
	{
		// Arrange
		List<Card> accounts = CardGenerator.GenerateList(2);
		List<CardResponse> expectedReturn = [.. accounts.Select(_mapper.ToResponse)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAllCardsQuery>(q => q.Offset == 0 && q.Limit == 50),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(new PagedResult<Card>(accounts, accounts.Count, 0, 50));

		// Act
		Results<Ok<CardListResponse>, BadRequest<string>> rawResult = await _controller.GetAllCards(null, 0, 50, null, null);

		// Assert
		Ok<CardListResponse> result = Assert.IsType<Ok<CardListResponse>>(rawResult.Result);
		CardListResponse actualReturn = result.Value!;

		actualReturn.Data.Should().BeEquivalentTo(expectedReturn);
		actualReturn.Total.Should().Be(accounts.Count);
		actualReturn.Offset.Should().Be(0);
		actualReturn.Limit.Should().Be(50);
	}

	[Fact]
	public async Task GetAllAccounts_PassesIsActiveToQuery()
	{
		// Arrange
		List<Card> accounts = CardGenerator.GenerateList(1);
		List<CardResponse> expectedReturn = [.. accounts.Select(_mapper.ToResponse)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAllCardsQuery>(q => q.Offset == 0 && q.Limit == 50 && q.IsActive == true),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(new PagedResult<Card>(accounts, accounts.Count, 0, 50));

		// Act
		Results<Ok<CardListResponse>, BadRequest<string>> rawResult = await _controller.GetAllCards(true, 0, 50, null, null);

		// Assert
		Ok<CardListResponse> result = Assert.IsType<Ok<CardListResponse>>(rawResult.Result);
		CardListResponse actualReturn = result.Value!;

		actualReturn.Data.Should().BeEquivalentTo(expectedReturn);
		actualReturn.Total.Should().Be(accounts.Count);
	}

	[Theory]
	[InlineData(-1, 50)]
	[InlineData(-100, 50)]
	public async Task GetAllAccounts_ReturnsBadRequest_WhenOffsetIsNegative(int offset, int limit)
	{
		// Act
		Results<Ok<CardListResponse>, BadRequest<string>> result = await _controller.GetAllCards(null, offset, limit, null, null);

		// Assert
		BadRequest<string> badRequestResult = Assert.IsType<BadRequest<string>>(result.Result);
		badRequestResult.Value.Should().Be("offset must be >= 0");
	}

	[Theory]
	[InlineData(0, 0)]
	[InlineData(0, -1)]
	[InlineData(0, 501)]
	public async Task GetAllAccounts_ReturnsBadRequest_WhenLimitIsOutOfRange(int offset, int limit)
	{
		// Act
		Results<Ok<CardListResponse>, BadRequest<string>> result = await _controller.GetAllCards(null, offset, limit, null, null);

		// Assert
		BadRequest<string> badRequestResult = Assert.IsType<BadRequest<string>>(result.Result);
		badRequestResult.Value.Should().Be("limit must be between 1 and 500");
	}

	[Fact]
	public async Task GetAllAccounts_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAllCardsQuery>(q => q.Offset == 0 && q.Limit == 50),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.GetAllCards(null, 0, 50, null, null);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task CreateAccount_ReturnsOkResult_WithCreatedAccount()
	{
		// Arrange
		Card account = CardGenerator.Generate();
		CardResponse expectedReturn = _mapper.ToResponse(account);

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateCardCommand>(c => c.Cards.Count == 1),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync([account]);

		CreateCardRequest controllerInput = CardDtoGenerator.GenerateCreateRequest();

		// Act
		Ok<CardResponse> result = await _controller.CreateCard(controllerInput);

		// Assert
		CardResponse actualReturn = result.Value!;

		actualReturn.Should().BeEquivalentTo(expectedReturn);
	}

	[Fact]
	public async Task CreateAccount_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		CreateCardRequest controllerInput = CardDtoGenerator.GenerateCreateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateCardCommand>(c => c.Cards.Count == 1),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.CreateCard(controllerInput);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task CreateAccounts_ReturnsOkResult_WithCreatedAccounts()
	{
		// Arrange
		List<Card> accounts = CardGenerator.GenerateList(2);
		List<CardResponse> expectedReturn = [.. accounts.Select(_mapper.ToResponse)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateCardCommand>(c => c.Cards.Count == accounts.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(accounts);

		List<CreateCardRequest> controllerInput = CardDtoGenerator.GenerateCreateRequestList(2);

		// Act
		Ok<List<CardResponse>> result = await _controller.CreateCards(controllerInput);

		// Assert
		List<CardResponse> actualReturn = result.Value!;

		actualReturn.Should().BeEquivalentTo(expectedReturn);
	}

	[Fact]
	public async Task CreateAccounts_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		List<CreateCardRequest> controllerInput = CardDtoGenerator.GenerateCreateRequestList(2);
		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateCardCommand>(c => c.Cards.Count == controllerInput.Count),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.CreateCards(controllerInput);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task UpdateAccount_ReturnsNoContent_WhenUpdateSucceeds()
	{
		// Arrange
		UpdateCardRequest controllerInput = CardDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateCardCommand>(c => c.Cards.Count == 1),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		Results<NoContent, NotFound> result = await _controller.UpdateCard(controllerInput.Id, controllerInput);

		// Assert
		Assert.IsType<NoContent>(result.Result);
	}

	[Fact]
	public async Task UpdateAccount_ReturnsNotFound_WhenUpdateFails()
	{
		// Arrange
		UpdateCardRequest controllerInput = CardDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateCardCommand>(c => c.Cards.Count == 1),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		Results<NoContent, NotFound> result = await _controller.UpdateCard(controllerInput.Id, controllerInput);

		// Assert
		Assert.IsType<NotFound>(result.Result);
	}

	[Fact]
	public async Task UpdateAccount_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		UpdateCardRequest controllerInput = CardDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateCardCommand>(c => c.Cards.Count == 1),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.UpdateCard(controllerInput.Id, controllerInput);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task UpdateAccounts_ReturnsNoContent_WhenUpdateSucceeds()
	{
		// Arrange
		List<UpdateCardRequest> controllerInput = CardDtoGenerator.GenerateUpdateRequestList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateCardCommand>(c => c.Cards.Count == controllerInput.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		Results<NoContent, NotFound> result = await _controller.UpdateCards(controllerInput);

		// Assert
		Assert.IsType<NoContent>(result.Result);
	}

	[Fact]
	public async Task UpdateAccounts_ReturnsNotFound_WhenUpdateFails()
	{
		// Arrange
		List<UpdateCardRequest> controllerInput = CardDtoGenerator.GenerateUpdateRequestList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateCardCommand>(c => c.Cards.Count == controllerInput.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		Results<NoContent, NotFound> result = await _controller.UpdateCards(controllerInput);

		// Assert
		Assert.IsType<NotFound>(result.Result);
	}

	[Fact]
	public async Task UpdateAccounts_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		List<UpdateCardRequest> controllerInput = CardDtoGenerator.GenerateUpdateRequestList(2);
		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateCardCommand>(c => c.Cards.Count == controllerInput.Count),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.UpdateCards(controllerInput);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task DeleteAccount_ReturnsNoContent_WhenDeleteSucceeds()
	{
		// Arrange
		Guid id = Guid.NewGuid();

		_accountServiceMock.Setup(s => s.GetTransactionCountByCardIdAsync(id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(0);

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteCardCommand>(c => c.Id == id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		Results<NoContent, NotFound, Conflict<object>> result = await _controller.DeleteCard(id);

		// Assert
		Assert.IsType<NoContent>(result.Result);
	}

	[Fact]
	public async Task DeleteAccount_ReturnsNotFound_WhenAccountDoesNotExist()
	{
		// Arrange
		Guid id = Guid.NewGuid();

		_accountServiceMock.Setup(s => s.GetTransactionCountByCardIdAsync(id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(0);

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteCardCommand>(c => c.Id == id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		Results<NoContent, NotFound, Conflict<object>> result = await _controller.DeleteCard(id);

		// Assert
		Assert.IsType<NotFound>(result.Result);
	}

	[Fact]
	public async Task DeleteAccount_ReturnsConflict_WhenTransactionsExist()
	{
		// Arrange
		Guid id = Guid.NewGuid();

		_accountServiceMock.Setup(s => s.GetTransactionCountByCardIdAsync(id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(5);

		// Act
		Results<NoContent, NotFound, Conflict<object>> result = await _controller.DeleteCard(id);

		// Assert
		Assert.IsType<Conflict<object>>(result.Result);
	}

	[Fact]
	public async Task DeleteAccount_ThrowsException_WhenMediatorFails()
	{
		// Arrange
		Guid id = Guid.NewGuid();

		_accountServiceMock.Setup(s => s.GetTransactionCountByCardIdAsync(id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(0);

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteCardCommand>(c => c.Id == id),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		// Act
		Func<Task> act = () => _controller.DeleteCard(id);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task MergeCards_WithFewerThanTwoCards_ReturnsBadRequest()
	{
		MergeCardsRequest request = new()
		{
			TargetAccountId = Guid.NewGuid(),
			SourceCardIds = [Guid.NewGuid()],
		};

		Results<Ok<MergeCardsResponse>, BadRequest<string>, NotFound, Conflict<MergeCardsConflictResponse>> result =
			await _controller.MergeCards(request);

		Assert.IsType<BadRequest<string>>(result.Result);
	}

	[Fact]
	public async Task MergeCards_WhenServiceSucceeds_ReturnsOk()
	{
		MergeCardsRequest request = new()
		{
			TargetAccountId = Guid.NewGuid(),
			SourceCardIds = [Guid.NewGuid(), Guid.NewGuid()],
		};

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<MergeCardsIntoAccountCommand>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(new MergeCardsResult(true, null));

		Results<Ok<MergeCardsResponse>, BadRequest<string>, NotFound, Conflict<MergeCardsConflictResponse>> result =
			await _controller.MergeCards(request);

		Ok<MergeCardsResponse> ok = Assert.IsType<Ok<MergeCardsResponse>>(result.Result);
		ok.Value!.Success.Should().BeTrue();
		_notifierMock.Verify(n => n.NotifyBulkChanged("card", "updated", It.IsAny<IEnumerable<Guid>>()), Times.Once);
	}

	[Fact]
	public async Task MergeCards_WhenServiceReturnsConflicts_ReturnsConflict()
	{
		MergeCardsRequest request = new()
		{
			TargetAccountId = Guid.NewGuid(),
			SourceCardIds = [Guid.NewGuid(), Guid.NewGuid()],
		};

		List<Application.Models.Merge.YnabMappingConflict> conflicts =
		[
			new(Guid.NewGuid(), "A", "b", "y1", "Y1"),
			new(Guid.NewGuid(), "B", "b", "y2", "Y2"),
		];

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<MergeCardsIntoAccountCommand>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(new MergeCardsResult(false, conflicts));

		Results<Ok<MergeCardsResponse>, BadRequest<string>, NotFound, Conflict<MergeCardsConflictResponse>> result =
			await _controller.MergeCards(request);

		Conflict<MergeCardsConflictResponse> conflict = Assert.IsType<Conflict<MergeCardsConflictResponse>>(result.Result);
		conflict.Value!.Conflicts.Should().HaveCount(2);
	}

	[Fact]
	public async Task MergeCards_WhenTargetNotFound_ReturnsNotFound()
	{
		MergeCardsRequest request = new()
		{
			TargetAccountId = Guid.NewGuid(),
			SourceCardIds = [Guid.NewGuid(), Guid.NewGuid()],
		};

		_mediatorMock.Setup(m => m.Send(
			It.IsAny<MergeCardsIntoAccountCommand>(),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new KeyNotFoundException("not found"));

		Results<Ok<MergeCardsResponse>, BadRequest<string>, NotFound, Conflict<MergeCardsConflictResponse>> result =
			await _controller.MergeCards(request);

		Assert.IsType<NotFound>(result.Result);
	}

	[Fact]
	public async Task MergeCards_WithInvalidCommand_ReturnsBadRequest()
	{
		MergeCardsRequest request = new()
		{
			TargetAccountId = Guid.Empty,
			SourceCardIds = [Guid.NewGuid(), Guid.NewGuid()],
		};

		Results<Ok<MergeCardsResponse>, BadRequest<string>, NotFound, Conflict<MergeCardsConflictResponse>> result =
			await _controller.MergeCards(request);

		Assert.IsType<BadRequest<string>>(result.Result);
	}
}
