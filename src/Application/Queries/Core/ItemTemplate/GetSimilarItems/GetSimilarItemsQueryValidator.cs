using FluentValidation;

namespace Application.Queries.Core.ItemTemplate.GetSimilarItems;

public class GetSimilarItemsQueryValidator : AbstractValidator<GetSimilarItemsQuery>
{
	public GetSimilarItemsQueryValidator()
	{
		RuleFor(x => x.SearchText)
			.NotEmpty().WithMessage("Search text is required.")
			.MinimumLength(2).WithMessage("Search text must be at least 2 characters.");

		RuleFor(x => x.Limit)
			.InclusiveBetween(1, 20).WithMessage("Limit must be between 1 and 20.");

		RuleFor(x => x.Threshold)
			.InclusiveBetween(0.0, 1.0).WithMessage("Threshold must be between 0.0 and 1.0.");
	}
}
