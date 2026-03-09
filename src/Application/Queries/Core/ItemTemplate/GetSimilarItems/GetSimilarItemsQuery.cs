using Application.Interfaces;

namespace Application.Queries.Core.ItemTemplate.GetSimilarItems;

public record GetSimilarItemsQuery(string SearchText, int Limit, double Threshold) : IQuery<IEnumerable<SimilarItemResult>>;
