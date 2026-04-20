using Application.Interfaces;
using Domain.NormalizedDescriptions;

namespace Application.Queries.NormalizedDescription.GetSettings;

public record GetNormalizedDescriptionSettingsQuery : IQuery<NormalizedDescriptionSettings>;
