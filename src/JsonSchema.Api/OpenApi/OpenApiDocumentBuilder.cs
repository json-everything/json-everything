using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema.Generation.SourceGeneration;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Assembles the fragments emitted across an application's assemblies into one document.
/// </summary>
internal static class OpenApiDocumentBuilder
{
	private const string _validationErrorType = "https://json-everything.net/errors/validation";
	private const string _validationProblemComponent = "ValidationProblemDetails";
	private const string _componentSchemaPointer = "#/components/schemas/";

	/// <summary>
	/// Collects the registered fragments contributing to a description.
	/// </summary>
	/// <param name="documentName">
	/// The description's name, or null for the default description.
	/// </param>
	/// <remarks>
	/// Each assembly emits one fragment per description it contributes to, so this is a
	/// matter of selecting the fragments carrying the name rather than filtering operations.
	/// </remarks>
	public static IReadOnlyList<OpenApiFragment> CollectFragments(string? documentName = null) =>
		[.. OpenApiFragmentRegistry.GetFragments().Where(x => x.Name == documentName)];

	/// <summary>
	/// Builds a document from the given fragments.
	/// </summary>
	/// <remarks>
	/// The title and version are seeded from the entry assembly; edit
	/// <see cref="OpenApiDocument.Info"/> to change them.
	/// </remarks>
	public static OpenApiDocument Build(IReadOnlyList<OpenApiFragment> fragments)
	{
		var name = Assembly.GetEntryAssembly()?.GetName();
		var title = name?.Name ?? "API";
		var version = name?.Version?.ToString(3) ?? "1.0.0";

		var document = new OpenApiDocument("3.1.1", new OpenApiInfo(title, version));

		var componentNames = new Dictionary<Type, string>();
		var schemas = new Dictionary<string, JsonSchema>();
		var enumFormat = EnumFormatResolver.Resolve(fragments);

		foreach (var fragment in fragments)
		{
			foreach (var kvp in fragment.ComponentNames)
			{
				componentNames[kvp.Key] = kvp.Value;
			}

			foreach (var kvp in fragment.Schemas)
			{
				schemas[kvp.Key] = kvp.Value;
			}
		}

		// Resolved before the operations are built, so the name the responses reference and
		// the key the schema is filed under cannot disagree.  A consumer type already holding
		// the name keeps it, and the shared schema takes a suffixed one.
		var validationProblemName = ClaimName(schemas, _validationProblemComponent);

		var paths = new PathCollection();
		var validationProblemUsed = false;

		foreach (var operation in fragments.SelectMany(x => x.Operations).OrderBy(x => x.Route))
		{
			AddOperation(paths, operation, componentNames, enumFormat, validationProblemName, ref validationProblemUsed);
		}

		// A description carries only the components its own operations reach, so splitting
		// the API does not give every description every schema.
		Prune(schemas, fragments, componentNames);

		// Added only when an endpoint references it, so an API with nothing validated carries
		// no unreferenced component.
		if (validationProblemUsed)
			schemas[validationProblemName] = BuildValidationProblemSchema();

		if (schemas.Count != 0)
			document.Components = new ComponentCollection { Schemas = schemas };

		if (paths.Count != 0)
			document.Paths = paths;

		return document;
	}

	private static void AddOperation(
		PathCollection paths,
		OpenApiFragmentOperation source,
		IReadOnlyDictionary<Type, string> componentNames,
		EnumFormat enumFormat,
		string validationProblemName,
		ref bool validationProblemUsed)
	{
		PathTemplate template = source.Route;

		if (!paths.TryGetValue(template, out var pathItem))
		{
			pathItem = new PathItem();
			paths[template] = pathItem;
		}

		var operation = new Operation
		{
			OperationId = source.OperationId,
			Summary = source.Summary,
			Description = source.Description,
			Tags = source.Tags.Count == 0 ? null : source.Tags,
			Parameters = BuildParameters(source, componentNames, enumFormat),
			RequestBody = BuildRequestBody(source, componentNames, enumFormat),
			Responses = BuildResponses(source, componentNames, enumFormat, validationProblemName, ref validationProblemUsed)
		};

		switch (source.Method)
		{
			case "get": pathItem.Get = operation; break;
			case "put": pathItem.Put = operation; break;
			case "post": pathItem.Post = operation; break;
			case "delete": pathItem.Delete = operation; break;
			case "patch": pathItem.Patch = operation; break;
			case "head": pathItem.Head = operation; break;
			case "options": pathItem.Options = operation; break;
			case "trace": pathItem.Trace = operation; break;
		}
	}

	private static IReadOnlyList<Parameter>? BuildParameters(
		OpenApiFragmentOperation source,
		IReadOnlyDictionary<Type, string> componentNames,
		EnumFormat enumFormat)
	{
		if (source.Parameters.Count == 0) return null;

		return
		[
			.. source.Parameters.Select(x => new Parameter(x.Name, ParseLocation(x.Location))
			{
				Description = x.Description,
				Required = x.Required,
				Schema = SchemaFor(x.Type, componentNames, enumFormat)
			})
		];
	}

	private static RequestBody? BuildRequestBody(
		OpenApiFragmentOperation source,
		IReadOnlyDictionary<Type, string> componentNames,
		EnumFormat enumFormat)
	{
		if (source.RequestBodyType is null) return null;

		return new RequestBody(new Dictionary<string, MediaType>
		{
			["application/json"] = new() { Schema = SchemaFor(source.RequestBodyType, componentNames, enumFormat) }
		})
		{
			Description = source.RequestBodyDescription,
			Required = true
		};
	}

	private static ResponseCollection BuildResponses(
		OpenApiFragmentOperation source,
		IReadOnlyDictionary<Type, string> componentNames,
		EnumFormat enumFormat,
		string validationProblemName,
		ref bool validationProblemUsed)
	{
		var responses = new ResponseCollection();

		foreach (var response in source.Responses)
		{
			var built = new Response(response.Description);

			var schema = SchemaFor(response.Type, componentNames, enumFormat);
			if (schema is not null)
			{
				built.Content = new Dictionary<string, MediaType>
				{
					["application/json"] = new() { Schema = schema }
				};
			}

			responses[(HttpStatusCode)response.StatusCode] = built;
		}

		// The validation middleware answers a malformed body with problem details before
		// the handler runs, so the response exists whether or not the handler declares it.
		if (source.RequestBodyIsValidated && !responses.ContainsKey(HttpStatusCode.BadRequest))
		{
			responses[HttpStatusCode.BadRequest] = BuildValidationErrorResponse(validationProblemName);
			validationProblemUsed = true;
		}

		return responses;
	}

	private static Response BuildValidationErrorResponse(string validationProblemName) =>
		new("The request body did not satisfy its schema.")
		{
			Content = new Dictionary<string, MediaType>
			{
				["application/problem+json"] = new() { Schema = Ref.To.Schema(validationProblemName) }
			}
		};

	// The middleware always answers with this shape, so every validated endpoint shares one
	// component rather than repeating the schema inline.
	private static JsonSchema BuildValidationProblemSchema() =>
		new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("type", new JsonSchemaBuilder().Type(SchemaValueType.String).Const(_validationErrorType)),
				("title", new JsonSchemaBuilder().Type(SchemaValueType.String)),
				("status", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
				("detail", new JsonSchemaBuilder().Type(SchemaValueType.String)),
				("errors", new JsonSchemaBuilder().Type(SchemaValueType.Object))
			);

	/// <summary>
	/// Removes the component schemas the given operations do not reach.
	/// </summary>
	/// <remarks>
	/// A schema can reference another, so this follows `$ref`s outward from the types the
	/// operations name rather than keeping only those types.
	/// </remarks>
	private static void Prune(
		Dictionary<string, JsonSchema> schemas,
		IReadOnlyList<OpenApiFragment> fragments,
		IReadOnlyDictionary<Type, string> componentNames)
	{
		var reachable = new HashSet<string>();
		var pending = new Queue<string>();

		foreach (var type in fragments.SelectMany(x => x.Operations).SelectMany(ReferencedTypes))
		{
			if (type is null) continue;
			if (!componentNames.TryGetValue(type, out var name)) continue;
			if (reachable.Add(name)) pending.Enqueue(name);
		}

		while (pending.Count != 0)
		{
			var name = pending.Dequeue();
			if (!schemas.TryGetValue(name, out var schema)) continue;

			foreach (var referenced in ComponentRefs(schema))
			{
				if (reachable.Add(referenced)) pending.Enqueue(referenced);
			}
		}

		foreach (var name in schemas.Keys.Where(x => !reachable.Contains(x)).ToArray())
		{
			schemas.Remove(name);
		}
	}

	private static IEnumerable<Type?> ReferencedTypes(OpenApiFragmentOperation operation)
	{
		yield return operation.RequestBodyType;

		foreach (var parameter in operation.Parameters)
		{
			yield return parameter.Type;
		}

		foreach (var response in operation.Responses)
		{
			yield return response.Type;
		}
	}

	private static IEnumerable<string> ComponentRefs(JsonSchema schema)
	{
		var node = JsonSerializer.SerializeToNode(schema, ApiSerializerContext.Default.JsonSchema);

		return node is null ? [] : ComponentRefs(node);
	}

	private static IEnumerable<string> ComponentRefs(JsonNode? node)
	{
		switch (node)
		{
			case JsonObject obj:
				foreach (var kvp in obj)
				{
					if (kvp.Key == "$ref" &&
						kvp.Value is JsonValue value &&
						value.TryGetValue<string>(out var reference) &&
						reference.StartsWith(_componentSchemaPointer, StringComparison.Ordinal))
					{
						yield return reference[_componentSchemaPointer.Length..];
						continue;
					}

					foreach (var nested in ComponentRefs(kvp.Value))
					{
						yield return nested;
					}
				}
				break;

			case JsonArray array:
				foreach (var item in array)
				{
					foreach (var nested in ComponentRefs(item))
					{
						yield return nested;
					}
				}
				break;
		}
	}

	private static string ClaimName(IReadOnlyDictionary<string, JsonSchema> schemas, string preferred)
	{
		if (!schemas.ContainsKey(preferred)) return preferred;

		var index = 2;
		while (schemas.ContainsKey($"{preferred}{index}")) index++;

		return $"{preferred}{index}";
	}

	private static JsonSchema? SchemaFor(Type? type, IReadOnlyDictionary<Type, string> componentNames, EnumFormat enumFormat)
	{
		if (type is null) return null;

		return componentNames.TryGetValue(type, out var componentName)
			? Ref.To.Schema(componentName)
			: PrimitiveSchemaFor(type, enumFormat);
	}

	private static JsonSchema? PrimitiveSchemaFor(Type type, EnumFormat enumFormat)
	{
		var unwrapped = Nullable.GetUnderlyingType(type) ?? type;

		if (unwrapped == typeof(string)) return new JsonSchemaBuilder().Type(SchemaValueType.String);
		if (unwrapped == typeof(bool)) return new JsonSchemaBuilder().Type(SchemaValueType.Boolean);
		if (unwrapped == typeof(Guid)) return new JsonSchemaBuilder().Type(SchemaValueType.String).Format(global::Json.Schema.Formats.Uuid);
		if (unwrapped == typeof(DateTime) || unwrapped == typeof(DateTimeOffset))
			return new JsonSchemaBuilder().Type(SchemaValueType.String).Format(global::Json.Schema.Formats.DateTime);

		if (unwrapped == typeof(int) || unwrapped == typeof(long) ||
			unwrapped == typeof(short) || unwrapped == typeof(byte))
		{
			return new JsonSchemaBuilder().Type(SchemaValueType.Integer);
		}

		if (unwrapped == typeof(double) || unwrapped == typeof(float) || unwrapped == typeof(decimal))
			return new JsonSchemaBuilder().Type(SchemaValueType.Number);

		if (unwrapped.IsEnum)
		{
			return enumFormat switch
			{
				EnumFormat.Values => new JsonSchemaBuilder().Type(SchemaValueType.Integer),
				EnumFormat.NamesAndValues => new JsonSchemaBuilder().AnyOf(
					new JsonSchemaBuilder().Enum(Enum.GetNames(unwrapped)),
					new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
				_ => new JsonSchemaBuilder().Enum(Enum.GetNames(unwrapped))
			};
		}

		return null;
	}

	private static ParameterLocation ParseLocation(string location) => location switch
	{
		"path" => ParameterLocation.Path,
		"header" => ParameterLocation.Header,
		"cookie" => ParameterLocation.Cookie,
		_ => ParameterLocation.Query
	};
}
