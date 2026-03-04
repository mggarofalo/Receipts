using API.Controllers.Core;
using API.Generated.Dtos;
using API.Mapping.Core;
using API.Services;
using Application.Commands.Category.Create;
using Application.Commands.Category.Delete;
using Application.Commands.Category.Restore;
using Application.Commands.Category.Update;
using Application.Models;
using Application.Queries.Core.Category;
using Domain.Core;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using SampleData.Domain.Core;
using SampleData.Dtos.Core;

namespace Presentation.API.Tests.Controllers.Core;

public class CategoriesControllerTests
{
	private readonly CategoryMapper _mapper;
	private readonly Mock<IMediator> _mediatorMock;
	private readonly Mock<ILogger<CategoriesController>> _loggerMock;
	private readonly Mock<IEntityChangeNotifier> _notifierMock;
	private readonly CategoriesController _controller;

	public CategoriesControllerTests()
	{
		_mediatorMock = new Mock<IMediator>();
		_mapper = new CategoryMapper();
		_loggerMock = ControllerTestHelpers.GetLoggerMock<CategoriesController>();
		_notifierMock = new Mock<IEntityChangeNotifier>();
		_controller = new CategoriesController(_mediatorMock.Object, _mapper, _loggerMock.Object, _notifierMock.Object);
	}

	// ── GetCategoryById ─────────────────────────────────────

	[Fact]
	public async Task GetCategoryById_ReturnsOkResult_WhenCategoryExists()
	{
		Category category = CategoryGenerator.Generate();
		CategoryResponse expectedReturn = _mapper.ToResponse(category);

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetCategoryByIdQuery>(q => q.Id == category.Id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(category);

		Results<Ok<CategoryResponse>, NotFound> result = await _controller.GetCategoryById(category.Id);

		Ok<CategoryResponse> okResult = Assert.IsType<Ok<CategoryResponse>>(result.Result);
		CategoryResponse actualReturn = Assert.IsType<CategoryResponse>(okResult.Value);
		actualReturn.Should().BeEquivalentTo(expectedReturn);
	}

	[Fact]
	public async Task GetCategoryById_ReturnsNotFound_WhenCategoryDoesNotExist()
	{
		Guid missingId = Guid.NewGuid();

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetCategoryByIdQuery>(q => q.Id == missingId),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync((Category?)null);

		Results<Ok<CategoryResponse>, NotFound> result = await _controller.GetCategoryById(missingId);

		Assert.IsType<NotFound>(result.Result);
	}

	[Fact]
	public async Task GetCategoryById_ThrowsException_WhenMediatorFails()
	{
		Guid id = Guid.NewGuid();

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetCategoryByIdQuery>(q => q.Id == id),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		Func<Task> act = () => _controller.GetCategoryById(id);

		await act.Should().ThrowAsync<Exception>();
	}

	// ── GetAllCategories ────────────────────────────────────

	[Fact]
	public async Task GetAllCategories_ReturnsOkResult_WithListOfCategories()
	{
		List<Category> categories = CategoryGenerator.GenerateList(2);
		List<CategoryResponse> expectedReturn = [.. categories.Select(_mapper.ToResponse)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAllCategoriesQuery>(q => q.Offset == 0 && q.Limit == 50),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(new PagedResult<Category>(categories, categories.Count, 0, 50));

		Ok<CategoryListResponse> result = await _controller.GetAllCategories(0, 50);

		CategoryListResponse actualReturn = result.Value!;
		actualReturn.Data.Should().BeEquivalentTo(expectedReturn);
		actualReturn.Total.Should().Be(categories.Count);
		actualReturn.Offset.Should().Be(0);
		actualReturn.Limit.Should().Be(50);
	}

	[Fact]
	public async Task GetAllCategories_ThrowsException_WhenMediatorFails()
	{
		_mediatorMock.Setup(m => m.Send(
			It.Is<GetAllCategoriesQuery>(q => q.Offset == 0 && q.Limit == 50),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		Func<Task> act = () => _controller.GetAllCategories(0, 50);

		await act.Should().ThrowAsync<Exception>();
	}

	// ── GetDeletedCategories ────────────────────────────────

	[Fact]
	public async Task GetDeletedCategories_ReturnsOkResult_WithListOfCategories()
	{
		List<Category> categories = CategoryGenerator.GenerateList(2);
		List<CategoryResponse> expectedReturn = [.. categories.Select(_mapper.ToResponse)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<GetDeletedCategoriesQuery>(q => q.Offset == 0 && q.Limit == 50),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(new PagedResult<Category>(categories, categories.Count, 0, 50));

		Ok<CategoryListResponse> result = await _controller.GetDeletedCategories(0, 50);

		CategoryListResponse actualReturn = result.Value!;
		actualReturn.Data.Should().BeEquivalentTo(expectedReturn);
		actualReturn.Total.Should().Be(categories.Count);
	}

	[Fact]
	public async Task GetDeletedCategories_ThrowsException_WhenMediatorFails()
	{
		_mediatorMock.Setup(m => m.Send(
			It.Is<GetDeletedCategoriesQuery>(q => q.Offset == 0 && q.Limit == 50),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		Func<Task> act = () => _controller.GetDeletedCategories(0, 50);

		await act.Should().ThrowAsync<Exception>();
	}

	// ── CreateCategory ──────────────────────────────────────

	[Fact]
	public async Task CreateCategory_ReturnsOkResult_WithCreatedCategory()
	{
		Category category = CategoryGenerator.Generate();
		CategoryResponse expectedReturn = _mapper.ToResponse(category);

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateCategoryCommand>(c => c.Categories.Count == 1),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync([category]);

		CreateCategoryRequest controllerInput = CategoryDtoGenerator.GenerateCreateRequest();

		Ok<CategoryResponse> result = await _controller.CreateCategory(controllerInput);

		CategoryResponse actualReturn = result.Value!;
		actualReturn.Should().BeEquivalentTo(expectedReturn);
	}

	[Fact]
	public async Task CreateCategory_ThrowsException_WhenMediatorFails()
	{
		CreateCategoryRequest controllerInput = CategoryDtoGenerator.GenerateCreateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateCategoryCommand>(c => c.Categories.Count == 1),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		Func<Task> act = () => _controller.CreateCategory(controllerInput);

		await act.Should().ThrowAsync<Exception>();
	}

	// ── CreateCategories (batch) ────────────────────────────

	[Fact]
	public async Task CreateCategories_ReturnsOkResult_WithCreatedCategories()
	{
		List<Category> categories = CategoryGenerator.GenerateList(2);
		List<CategoryResponse> expectedReturn = [.. categories.Select(_mapper.ToResponse)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateCategoryCommand>(c => c.Categories.Count == categories.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(categories);

		List<CreateCategoryRequest> controllerInput = CategoryDtoGenerator.GenerateCreateRequestList(2);

		Ok<List<CategoryResponse>> result = await _controller.CreateCategories(controllerInput);

		List<CategoryResponse> actualReturn = result.Value!;
		actualReturn.Should().BeEquivalentTo(expectedReturn);
	}

	[Fact]
	public async Task CreateCategories_ThrowsException_WhenMediatorFails()
	{
		List<CreateCategoryRequest> controllerInput = CategoryDtoGenerator.GenerateCreateRequestList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<CreateCategoryCommand>(c => c.Categories.Count == controllerInput.Count),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		Func<Task> act = () => _controller.CreateCategories(controllerInput);

		await act.Should().ThrowAsync<Exception>();
	}

	// ── UpdateCategory ──────────────────────────────────────

	[Fact]
	public async Task UpdateCategory_ReturnsNoContent_WhenUpdateSucceeds()
	{
		UpdateCategoryRequest controllerInput = CategoryDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateCategoryCommand>(c => c.Categories.Count == 1),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		Results<NoContent, NotFound> result = await _controller.UpdateCategory(controllerInput.Id, controllerInput);

		Assert.IsType<NoContent>(result.Result);
	}

	[Fact]
	public async Task UpdateCategory_ReturnsNotFound_WhenUpdateFails()
	{
		UpdateCategoryRequest controllerInput = CategoryDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateCategoryCommand>(c => c.Categories.Count == 1),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		Results<NoContent, NotFound> result = await _controller.UpdateCategory(controllerInput.Id, controllerInput);

		Assert.IsType<NotFound>(result.Result);
	}

	[Fact]
	public async Task UpdateCategory_ThrowsException_WhenMediatorFails()
	{
		UpdateCategoryRequest controllerInput = CategoryDtoGenerator.GenerateUpdateRequest();

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateCategoryCommand>(c => c.Categories.Count == 1),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		Func<Task> act = () => _controller.UpdateCategory(controllerInput.Id, controllerInput);

		await act.Should().ThrowAsync<Exception>();
	}

	// ── UpdateCategories (batch) ────────────────────────────

	[Fact]
	public async Task UpdateCategories_ReturnsNoContent_WhenUpdateSucceeds()
	{
		List<UpdateCategoryRequest> controllerInput = CategoryDtoGenerator.GenerateUpdateRequestList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateCategoryCommand>(c => c.Categories.Count == controllerInput.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		Results<NoContent, NotFound> result = await _controller.UpdateCategories(controllerInput);

		Assert.IsType<NoContent>(result.Result);
	}

	[Fact]
	public async Task UpdateCategories_ReturnsNotFound_WhenUpdateFails()
	{
		List<UpdateCategoryRequest> controllerInput = CategoryDtoGenerator.GenerateUpdateRequestList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateCategoryCommand>(c => c.Categories.Count == controllerInput.Count),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		Results<NoContent, NotFound> result = await _controller.UpdateCategories(controllerInput);

		Assert.IsType<NotFound>(result.Result);
	}

	[Fact]
	public async Task UpdateCategories_ThrowsException_WhenMediatorFails()
	{
		List<UpdateCategoryRequest> controllerInput = CategoryDtoGenerator.GenerateUpdateRequestList(2);

		_mediatorMock.Setup(m => m.Send(
			It.Is<UpdateCategoryCommand>(c => c.Categories.Count == controllerInput.Count),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		Func<Task> act = () => _controller.UpdateCategories(controllerInput);

		await act.Should().ThrowAsync<Exception>();
	}

	// ── DeleteCategories ────────────────────────────────────

	[Fact]
	public async Task DeleteCategories_ReturnsNoContent_WhenDeleteSucceeds()
	{
		List<Guid> ids = [.. CategoryGenerator.GenerateList(2).Select(c => c.Id)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteCategoryCommand>(c => c.Ids.SequenceEqual(ids)),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		Results<NoContent, NotFound> result = await _controller.DeleteCategories(ids);

		Assert.IsType<NoContent>(result.Result);
	}

	[Fact]
	public async Task DeleteCategories_ReturnsNotFound_WhenDeleteFails()
	{
		List<Guid> ids = [CategoryGenerator.Generate().Id];

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteCategoryCommand>(c => c.Ids.SequenceEqual(ids)),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		Results<NoContent, NotFound> result = await _controller.DeleteCategories(ids);

		Assert.IsType<NotFound>(result.Result);
	}

	[Fact]
	public async Task DeleteCategories_ThrowsException_WhenMediatorFails()
	{
		List<Guid> ids = [.. CategoryGenerator.GenerateList(2).Select(c => c.Id)];

		_mediatorMock.Setup(m => m.Send(
			It.Is<DeleteCategoryCommand>(c => c.Ids.SequenceEqual(ids)),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		Func<Task> act = () => _controller.DeleteCategories(ids);

		await act.Should().ThrowAsync<Exception>();
	}

	// ── RestoreCategory ─────────────────────────────────────

	[Fact]
	public async Task RestoreCategory_ReturnsNoContent_WhenSuccessful()
	{
		Guid id = Guid.NewGuid();

		_mediatorMock.Setup(m => m.Send(
			It.Is<RestoreCategoryCommand>(c => c.Id == id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		Results<NoContent, NotFound> result = await _controller.RestoreCategory(id);

		Assert.IsType<NoContent>(result.Result);
	}

	[Fact]
	public async Task RestoreCategory_ReturnsNotFound_WhenEntityDoesNotExist()
	{
		Guid id = Guid.NewGuid();

		_mediatorMock.Setup(m => m.Send(
			It.Is<RestoreCategoryCommand>(c => c.Id == id),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		Results<NoContent, NotFound> result = await _controller.RestoreCategory(id);

		Assert.IsType<NotFound>(result.Result);
	}

	[Fact]
	public async Task RestoreCategory_ThrowsException_WhenMediatorFails()
	{
		Guid id = Guid.NewGuid();

		_mediatorMock.Setup(m => m.Send(
			It.Is<RestoreCategoryCommand>(c => c.Id == id),
			It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		Func<Task> act = () => _controller.RestoreCategory(id);

		await act.Should().ThrowAsync<Exception>();
	}
}
