using System.Text.Json.Serialization;

namespace Infrastructure.Services;

internal sealed record OllamaGenerateResponse(
	[property: JsonPropertyName("model")] string? Model,
	[property: JsonPropertyName("response")] string? Response,
	[property: JsonPropertyName("done")] bool Done
);
