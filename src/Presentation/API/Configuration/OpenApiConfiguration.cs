using System.Text.Json;
using System.Text.Json.Nodes;
using API.Middleware;
using Scalar.AspNetCore;

namespace API.Configuration;

public static class OpenApiConfiguration
{
	public static IServiceCollection AddOpenApiServices(this IServiceCollection services)
	{
		services.AddOpenApi(options =>
		{
			// The built-in ASP.NET Core OpenAPI generator emits enum schemas using the
			// raw C# member names (PascalCase). The API is configured to serialize enum
			// values using JsonNamingPolicy.CamelCase (see ApplicationConfiguration), so
			// the wire format is camelCase. Without this transformer, the generated
			// openapi/generated/API.json drifts from openapi/spec.yaml and the runtime
			// wire format. Rewrite every enum value to camelCase so the generated spec
			// faithfully reflects the runtime.
			options.AddSchemaTransformer((schema, context, cancellationToken) =>
			{
				if (schema.Enum is { Count: > 0 })
				{
					List<JsonNode> rewritten = new(schema.Enum.Count);
					foreach (JsonNode? value in schema.Enum)
					{
						if (value is JsonValue jsonValue
							&& jsonValue.TryGetValue(out string? str)
							&& !string.IsNullOrEmpty(str))
						{
							rewritten.Add(JsonValue.Create(JsonNamingPolicy.CamelCase.ConvertName(str))!);
						}
						else if (value is not null)
						{
							rewritten.Add(value.DeepClone());
						}
					}
					schema.Enum = rewritten;
				}
				return Task.CompletedTask;
			});
		});
		return services;
	}

	public static WebApplication UseOpenApiServices(this WebApplication app)
	{
		if (app.Environment.IsDevelopment())
		{
			app.MapOpenApi();
			app.MapScalarApiReference();
			// Response validation is registered separately via UseOpenApiResponseValidation()
			// AFTER UseApplicationServices() (which includes UseResponseCompression()).
			// This ensures the validation middleware reads uncompressed response bodies.
		}
		else
		{
			app.UseHsts();
		}

		return app;
	}
}
