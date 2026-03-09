using FluentValidation;

namespace Application.Queries.Core.ItemTemplate.GetCategoryRecommendations;

public class GetCategoryRecommendationsQueryValidator : AbstractValidator<GetCategoryRecommendationsQuery>
{
	public GetCategoryRecommendationsQueryValidator()
	{
		RuleFor(x => x.Description)
			.NotEmpty().WithMessage("Description is required.")
			.MinimumLength(2).WithMessage("Description must be at least 2 characters.");

		RuleFor(x => x.Limit)
			.InclusiveBetween(1, 20).WithMessage("Limit must be between 1 and 20.");
	}
}
