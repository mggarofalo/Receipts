using System.Text.Json.Serialization;

namespace Infrastructure.Services;

internal sealed record OllamaGenerateRequest(
	[property: JsonPropertyName("model")] string Model,
	[property: JsonPropertyName("prompt")] string Prompt,
	[property: JsonPropertyName("images")] IReadOnlyList<string> Images,
	[property: JsonPropertyName("format")] string Format = "json",
	[property: JsonPropertyName("stream")] bool Stream = false
);
