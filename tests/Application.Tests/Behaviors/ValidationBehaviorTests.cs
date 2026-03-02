using Application.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
	public record TestRequest(string Name) : IRequest<string>;

	public class PassingValidator : AbstractValidator<TestRequest>
	{
	}

	public class FailingNameRequiredValidator : AbstractValidator<TestRequest>
	{
		public FailingNameRequiredValidator()
		{
			RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
		}
	}

	public class FailingNameLengthValidator : AbstractValidator<TestRequest>
	{
		public FailingNameLengthValidator()
		{
			RuleFor(x => x.Name).MinimumLength(3).WithMessage("Name must be at least 3 characters");
		}
	}

	[Fact]
	public async Task Handle_NoValidators_CallsNext()
	{
		// Arrange
		List<IValidator<TestRequest>> validators = [];
		ValidationBehavior<TestRequest, string> behavior = new(validators);
		TestRequest request = new("test");

		// Act
		string result = await behavior.Handle(
			request,
			_ => Task.FromResult("ok"),
			CancellationToken.None);

		// Assert
		result.Should().Be("ok");
	}

	[Fact]
	public async Task Handle_ValidRequest_CallsNext()
	{
		// Arrange
		List<IValidator<TestRequest>> validators = [new PassingValidator()];
		ValidationBehavior<TestRequest, string> behavior = new(validators);
		TestRequest request = new("test");

		// Act
		string result = await behavior.Handle(
			request,
			_ => Task.FromResult("ok"),
			CancellationToken.None);

		// Assert
		result.Should().Be("ok");
	}

	[Fact]
	public async Task Handle_InvalidRequest_ThrowsValidationException()
	{
		// Arrange
		List<IValidator<TestRequest>> validators = [new FailingNameRequiredValidator()];
		ValidationBehavior<TestRequest, string> behavior = new(validators);
		TestRequest request = new("");

		// Act
		Func<Task> act = () => behavior.Handle(
			request,
			_ => Task.FromResult("ok"),
			CancellationToken.None);

		// Assert
		ValidationException exception = (await act.Should().ThrowAsync<ValidationException>()).Which;
		exception.Errors.Should().ContainSingle(e => e.PropertyName == "Name");
	}

	[Fact]
	public async Task Handle_MultipleValidators_AggregatesErrors()
	{
		// Arrange
		List<IValidator<TestRequest>> validators =
		[
			new FailingNameRequiredValidator(),
			new FailingNameLengthValidator()
		];
		ValidationBehavior<TestRequest, string> behavior = new(validators);
		TestRequest request = new("");

		// Act
		Func<Task> act = () => behavior.Handle(
			request,
			_ => Task.FromResult("ok"),
			CancellationToken.None);

		// Assert
		ValidationException exception = (await act.Should().ThrowAsync<ValidationException>()).Which;
		exception.Errors.Should().HaveCount(2);
	}
}
