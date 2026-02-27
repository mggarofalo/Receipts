using NJsonSchema;
using System.Text;
using System.Text.Json;

namespace API.Middleware;

/// <summary>
/// Development-only middleware that validates API response bodies against the OpenAPI spec.
/// Catches serialization mismatches (casing, dates, enums), missing required fields,
/// and other divergences between spec and implementation.
/// </summary>
public class OpenApiResponseValidationMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<OpenApiResponseValidationMiddleware> _logger;
	private readonly bool _throwOnFailure;
	private readonly Dictionary<string, JsonSchema> _schemaCache = [];
	private readonly JsonDocument? _specDocument;

	public OpenApiResponseValidationMiddleware(
		RequestDelegate next,
		ILogger<OpenApiResponseValidationMiddleware> logger,
		IWebHostEnvironment env,
		bool throwOnFailure = false)
	{
		_next = next;
		_logger = logger;
		_throwOnFailure = throwOnFailure;

		string specPath = Path.Combine(env.ContentRootPath, "..", "..", "..", "openapi", "generated", "API.json");
		if (File.Exists(specPath))
		{
			string json = File.ReadAllText(specPath);
			_specDocument = JsonDocument.Parse(json);

			if (_specDocument.RootElement.TryGetProperty("paths", out JsonElement paths))
			{
				int pathCount = paths.EnumerateObject().Count();
				_logger.LogInformation(
					"OpenAPI response validation middleware loaded spec with {PathCount} paths",
					pathCount);
			}
		}
		else
		{
			_logger.LogWarning("OpenAPI spec not found at {Path}. Response validation disabled", specPath);
		}
	}

	public async Task InvokeAsync(HttpContext context)
	{
		if (_specDocument is null)
		{
			await _next(context);
			return;
		}

		// Skip non-API requests and OpenAPI/health endpoints
		string path = context.Request.Path.Value ?? "";
		if (!path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase)
			|| path.StartsWith("/api/openapi", StringComparison.OrdinalIgnoreCase))
		{
			await _next(context);
			return;
		}

		// Buffer the response body so we can read it after the handler writes it
		Stream originalBody = context.Response.Body;
		using MemoryStream bufferedBody = new();
		context.Response.Body = bufferedBody;

		try
		{
			await _next(context);

			if (context.Response.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true
				&& context.Response.StatusCode >= 200
				&& context.Response.StatusCode < 300)
			{
				bufferedBody.Seek(0, SeekOrigin.Begin);
				string responseJson = await new StreamReader(bufferedBody, Encoding.UTF8).ReadToEndAsync();

				if (!string.IsNullOrWhiteSpace(responseJson))
				{
					await ValidateResponseAsync(context, responseJson);
				}
			}
		}
		finally
		{
			// Copy the buffered response back to the original stream
			bufferedBody.Seek(0, SeekOrigin.Begin);
			await bufferedBody.CopyToAsync(originalBody);
			context.Response.Body = originalBody;
		}
	}

	private async Task ValidateResponseAsync(HttpContext context, string responseJson)
	{
		string method = context.Request.Method.ToLowerInvariant();
		string statusCode = context.Response.StatusCode.ToString();
		string requestPath = context.Request.Path.Value ?? "";

		// Find the matching response schema JSON from the spec
		string cacheKey = $"{method}:{requestPath}:{statusCode}";
		JsonElement? schemaElement = FindResponseSchemaElement(requestPath, method, statusCode);
		if (schemaElement is null)
		{
			return; // No schema defined — skip
		}

		try
		{
			JsonSchema schema = await GetOrCreateSchemaAsync(schemaElement.Value, cacheKey);
			ICollection<NJsonSchema.Validation.ValidationError> errors = schema.Validate(responseJson);

			if (errors.Count > 0)
			{
				string endpoint = $"{method.ToUpperInvariant()} {requestPath}";
				string errorDetails = string.Join("; ", errors.Select(e => $"{e.Path}: {e.Kind}"));

				_logger.LogWarning(
					"OpenAPI response validation failed for {Endpoint} ({StatusCode}): {Errors}",
					endpoint, statusCode, errorDetails);

				if (_throwOnFailure)
				{
					throw new InvalidOperationException(
						$"OpenAPI response validation failed for {endpoint}: {errorDetails}");
				}
			}
		}
		catch (Exception ex) when (ex is not InvalidOperationException)
		{
			_logger.LogDebug(ex, "Error during OpenAPI response validation for {Path}", requestPath);
		}
	}

	private JsonElement? FindResponseSchemaElement(string requestPath, string method, string statusCode)
	{
		if (_specDocument is null
			|| !_specDocument.RootElement.TryGetProperty("paths", out JsonElement paths))
		{
			return null;
		}

		// Find the matching path template
		foreach (JsonProperty pathEntry in paths.EnumerateObject())
		{
			if (!PathMatchesTemplate(requestPath, pathEntry.Name))
			{
				continue;
			}

			if (!pathEntry.Value.TryGetProperty(method, out JsonElement operation))
			{
				continue;
			}

			if (!operation.TryGetProperty("responses", out JsonElement responses))
			{
				continue;
			}

			// Try exact status code, then "default"
			JsonElement responseElement;
			if (!responses.TryGetProperty(statusCode, out responseElement)
				&& !responses.TryGetProperty("default", out responseElement))
			{
				continue;
			}

			if (responseElement.TryGetProperty("content", out JsonElement content)
				&& content.TryGetProperty("application/json", out JsonElement mediaType)
				&& mediaType.TryGetProperty("schema", out JsonElement schema))
			{
				return schema;
			}
		}

		return null;
	}

	private static bool PathMatchesTemplate(string requestPath, string template)
	{
		string[] requestSegments = requestPath.Trim('/').Split('/');
		string[] templateSegments = template.Trim('/').Split('/');

		if (requestSegments.Length != templateSegments.Length)
		{
			return false;
		}

		for (int i = 0; i < templateSegments.Length; i++)
		{
			string templateSeg = templateSegments[i];
			if (templateSeg.StartsWith('{') && templateSeg.EndsWith('}'))
			{
				continue;
			}

			if (!string.Equals(requestSegments[i], templateSeg, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
		}

		return true;
	}

	private async Task<JsonSchema> GetOrCreateSchemaAsync(JsonElement schemaElement, string cacheKey)
	{
		if (_schemaCache.TryGetValue(cacheKey, out JsonSchema? cached))
		{
			return cached;
		}

		// Resolve $ref and build a self-contained JSON Schema string
		string schemaJson = BuildResolvedSchemaJson(schemaElement);
		JsonSchema schema = await JsonSchema.FromJsonAsync(schemaJson);
		_schemaCache[cacheKey] = schema;
		return schema;
	}

	private string BuildResolvedSchemaJson(JsonElement element)
	{
		JsonElement resolved = ResolveRefs(element, []);
		return resolved.GetRawText();
	}

	private JsonElement ResolveRefs(JsonElement element, HashSet<string> visited)
	{
		if (element.ValueKind == JsonValueKind.Object)
		{
			// Check for $ref
			if (element.TryGetProperty("$ref", out JsonElement refElement))
			{
				string refPath = refElement.GetString() ?? "";
				if (refPath.StartsWith("#/") && !visited.Contains(refPath))
				{
					visited.Add(refPath);
					JsonElement? resolved = NavigateJsonPointer(refPath);
					if (resolved.HasValue)
					{
						return ResolveRefs(resolved.Value, visited);
					}
				}

				// Circular ref or unresolvable — return empty object
				return JsonDocument.Parse("{}").RootElement;
			}

			// Recursively resolve all properties
			using var memoryStream = new MemoryStream();
			using (var writer = new Utf8JsonWriter(memoryStream))
			{
				writer.WriteStartObject();
				foreach (JsonProperty prop in element.EnumerateObject())
				{
					writer.WritePropertyName(prop.Name);
					JsonElement resolvedValue = ResolveRefs(prop.Value, visited);
					resolvedValue.WriteTo(writer);
				}
				writer.WriteEndObject();
			}

			return JsonDocument.Parse(memoryStream.ToArray()).RootElement;
		}

		if (element.ValueKind == JsonValueKind.Array)
		{
			using var memoryStream = new MemoryStream();
			using (var writer = new Utf8JsonWriter(memoryStream))
			{
				writer.WriteStartArray();
				foreach (JsonElement item in element.EnumerateArray())
				{
					JsonElement resolvedItem = ResolveRefs(item, visited);
					resolvedItem.WriteTo(writer);
				}
				writer.WriteEndArray();
			}

			return JsonDocument.Parse(memoryStream.ToArray()).RootElement;
		}

		return element;
	}

	private JsonElement? NavigateJsonPointer(string pointer)
	{
		if (_specDocument is null || !pointer.StartsWith("#/"))
		{
			return null;
		}

		string[] segments = pointer[2..].Split('/');
		JsonElement current = _specDocument.RootElement;

		foreach (string segment in segments)
		{
			// Unescape JSON Pointer encoding
			string unescaped = segment.Replace("~1", "/").Replace("~0", "~");

			if (current.ValueKind != JsonValueKind.Object
				|| !current.TryGetProperty(unescaped, out JsonElement next))
			{
				return null;
			}

			current = next;
		}

		return current;
	}
}

/// <summary>
/// Configuration options for OpenAPI response validation.
/// </summary>
public class OpenApiResponseValidationOptions
{
	/// <summary>
	/// When true, validation failures throw an exception instead of just logging.
	/// Default: false (log warnings only).
	/// </summary>
	public bool ThrowOnFailure { get; set; }
}

public static class OpenApiResponseValidationExtensions
{
	/// <summary>
	/// Adds OpenAPI response validation middleware (development mode only).
	/// Validates that API responses match the OpenAPI spec schemas.
	/// </summary>
	public static WebApplication UseOpenApiResponseValidation(this WebApplication app, Action<OpenApiResponseValidationOptions>? configure = null)
	{
		if (!app.Environment.IsDevelopment())
		{
			return app;
		}

		OpenApiResponseValidationOptions options = new();
		configure?.Invoke(options);

		app.UseMiddleware<OpenApiResponseValidationMiddleware>(options.ThrowOnFailure);
		return app;
	}
}
