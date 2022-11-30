using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Represents a JSON Schema.
/// </summary>
[JsonConverter(typeof(SchemaJsonConverter))]
[DebuggerDisplay("{ToDebugString()}")]
public class JsonSchema : IEquatable<JsonSchema>
{
	/// <summary>
	/// The empty schema `{}`.  Functionally equivalent to <see cref="True"/>.
	/// </summary>
	public static readonly JsonSchema Empty = new(Enumerable.Empty<IJsonSchemaKeyword>());
	/// <summary>
	/// The `true` schema.  Passes all instances.
	/// </summary>
	public static readonly JsonSchema True = new(true) { BaseUri = new("https://json-schema.org/true") };
	/// <summary>
	/// The `false` schema.  Fails all instances.
	/// </summary>
	public static readonly JsonSchema False = new(false) { BaseUri = new("https://json-schema.org/false") };

	/// <summary>
	/// Gets the keywords contained in the schema.  Only populated for non-boolean schemas.
	/// </summary>
	public IReadOnlyCollection<IJsonSchemaKeyword>? Keywords { get; }

	/// <summary>
	/// For boolean schemas, gets the value.  Null if the schema isn't a boolean schema.
	/// </summary>
	public bool? BoolValue { get; }

	public Uri BaseUri { get; private set; } = GenerateBaseUri();

	public bool IsResourceRoot { get; private set; }

	public SpecVersion DeclaredVersion { get; private set; }

	internal Dictionary<string, (JsonSchema Schema, bool IsDynamic)> Anchors { get; } = new();
	internal JsonSchema? RecursiveAnchor { get; set; }

	private JsonSchema(bool value)
	{
		BoolValue = value;
	}
	internal JsonSchema(IEnumerable<IJsonSchemaKeyword> keywords)
	{
		Keywords = keywords.ToArray();
	}

	/// <summary>
	/// Loads text from a file and deserializes a <see cref="JsonSchema"/>.
	/// </summary>
	/// <param name="fileName">The filename to load, URL-decoded.</param>
	/// <param name="options">Serializer options.</param>
	/// <returns>A new <see cref="JsonSchema"/>.</returns>
	/// <exception cref="JsonException">Could not deserialize a portion of the schema.</exception>
	/// <remarks>The filename needs to not be URL-encoded as <see cref="Uri"/> attempts to encode it.</remarks>
	public static JsonSchema FromFile(string fileName, JsonSerializerOptions? options = null)
	{
		var text = File.ReadAllText(fileName);
		var schema = FromText(text, options);
		schema.BaseUri = new Uri(Path.GetFullPath(fileName));
		return schema;
	}

	/// <summary>
	/// Deserializes a <see cref="JsonSchema"/> from text.
	/// </summary>
	/// <param name="jsonText">The text to parse.</param>
	/// <param name="options">Serializer options.</param>
	/// <returns>A new <see cref="JsonSchema"/>.</returns>
	/// <exception cref="JsonException">Could not deserialize a portion of the schema.</exception>
	public static JsonSchema FromText(string jsonText, JsonSerializerOptions? options = null)
	{
		return JsonSerializer.Deserialize<JsonSchema>(jsonText, options)!;
	}

	/// <summary>
	/// Deserializes a <see cref="JsonSchema"/> from a stream.
	/// </summary>
	/// <param name="source">A stream.</param>
	/// <param name="options">Serializer options.</param>
	/// <returns>A new <see cref="JsonSchema"/>.</returns>
	public static ValueTask<JsonSchema> FromStream(Stream source, JsonSerializerOptions? options = null)
	{
		return JsonSerializer.DeserializeAsync<JsonSchema>(source, options)!;
	}

	private static Uri GenerateBaseUri() => new($"https://json-everything.net/{Guid.NewGuid():N}");

	/// <summary>
	/// Evaluates an instance against this schema.
	/// </summary>
	/// <param name="root">The root instance.</param>
	/// <param name="options">The options to use for this evaluation.</param>
	/// <returns>A <see cref="EvaluationResults"/> that provides the outcome of the evaluation.</returns>
	public EvaluationResults Evaluate(JsonNode? root, EvaluationOptions? options = null)
	{
		options = EvaluationOptions.From(options ?? EvaluationOptions.Default);

		options.Log.Write(() => "Registering subschemas.");
		// BaseUri may change if $id is present
		options.EvaluatingAs = DetermineSpecVersion(this, options.SchemaRegistry, options.EvaluateAs);
		PopulateBaseUris(this, this, BaseUri, options.SchemaRegistry, options.EvaluatingAs, true);

		var context = new EvaluationContext(options, BaseUri, root, this);

		options.Log.Write(() => "Beginning evaluation.");
		context.Evaluate();

		options.Log.Write(() => "Transforming output.");
		var results = context.LocalResult;
		switch (options.OutputFormat)
		{
			case OutputFormat.Flag:
				results.ToFlag();
				break;
			case OutputFormat.Basic:
				results.ToBasic();
				break;
			case OutputFormat.Hierarchical:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		options.Log.Write(() => $"Evaluation complete: {results.IsValid.GetValidityString()}");
		return results;
	}

	internal static void Initialize(JsonSchema schema, SchemaRegistry registry, Uri? baseUri = null)
	{
		PopulateBaseUris(schema, schema, baseUri ?? schema.BaseUri, registry, DetermineSpecVersion(schema, registry, SpecVersion.Unspecified), true);
	}

	private static SpecVersion DetermineSpecVersion(JsonSchema schema, SchemaRegistry registry, SpecVersion desiredDraft)
	{
		if (schema.BoolValue.HasValue) return SpecVersion.DraftNext;

		var schemaKeyword = (SchemaKeyword?)schema.Keywords!.FirstOrDefault(x => x is SchemaKeyword);
		if (schemaKeyword != null)
		{
			var metaSchemaId = schemaKeyword.Schema;
			while (metaSchemaId != null)
			{
				var version = metaSchemaId.OriginalString switch
				{
					MetaSchemas.Draft6IdValue => SpecVersion.Draft6,
					MetaSchemas.Draft7IdValue => SpecVersion.Draft7,
					MetaSchemas.Draft201909IdValue => SpecVersion.Draft201909,
					MetaSchemas.Draft202012IdValue => SpecVersion.Draft202012,
					MetaSchemas.DraftNextIdValue => SpecVersion.DraftNext,
					_ => SpecVersion.Unspecified
				};
				if (version != SpecVersion.Unspecified)
				{
					schema.DeclaredVersion = version;
					return version;
				}

				var metaSchema = registry.Get(metaSchemaId);
				if (metaSchema == null)
					throw new JsonSchemaException("Cannot resolve custom meta-schema.  Make sure meta-schemas are registered in the global registry.");

				var newMetaSchemaId = metaSchema.Keywords!.OfType<SchemaKeyword>().FirstOrDefault()?.Schema;
				if (newMetaSchemaId == metaSchemaId)
					throw new JsonSchemaException("Custom meta-schema `$schema` keywords must eventually resolve to a meta-schema for a supported specification version.");

				metaSchemaId = newMetaSchemaId;
			}
		}

		if (desiredDraft != SpecVersion.Unspecified) return desiredDraft;

		var allDraftsArray = Enum.GetValues(typeof(SpecVersion)).Cast<SpecVersion>().ToArray();
		var allDrafts = allDraftsArray.Aggregate(SpecVersion.Unspecified, (a, x) => a | x);
		var commonDrafts = schema.Keywords!.Aggregate(allDrafts, (a, x) => a & x.VersionsSupported());
		var candidates = allDraftsArray.Where(x => commonDrafts.HasFlag(x)).ToArray();

		return candidates.Any() ? candidates.Max() : SpecVersion.DraftNext;
	}

	private static void PopulateBaseUris(JsonSchema schema, JsonSchema resourceRoot, Uri currentBaseUri, SchemaRegistry registry, SpecVersion evaluatingAs, bool selfRegister = false)
	{
		if (schema.BoolValue.HasValue) return;
		if (evaluatingAs is SpecVersion.Draft6 or SpecVersion.Draft7 &&
		    schema.Keywords!.Any(x => x is RefKeyword))
		{
			schema.BaseUri = currentBaseUri;
			return;
		}

		var idKeyword = schema.Keywords!.OfType<IdKeyword>().FirstOrDefault();

		if (idKeyword == null)
		{
			schema.BaseUri = currentBaseUri;
			if (selfRegister)
				registry.RegisterSchema(schema.BaseUri, schema);
		}
		else
		{
			if (evaluatingAs is SpecVersion.Draft6 or SpecVersion.Draft7 &&
			    idKeyword.Id.OriginalString[0] == '#' &&
			    AnchorKeyword.AnchorPattern.IsMatch(idKeyword.Id.OriginalString.Substring(1)))
			{
				schema.BaseUri = currentBaseUri;
				resourceRoot.Anchors[idKeyword.Id.OriginalString.Substring(1)] = (schema, false);
			}
			else
			{
				schema.IsResourceRoot = true;
				schema.DeclaredVersion = DetermineSpecVersion(schema, registry, evaluatingAs);
				resourceRoot = schema;
				schema.BaseUri = new Uri(currentBaseUri, idKeyword.Id);
				registry.RegisterSchema(schema.BaseUri, schema);
			}
		}

		var anchorKeyword = schema.Keywords!.OfType<AnchorKeyword>().FirstOrDefault();
		if (anchorKeyword != null)
			resourceRoot.Anchors[anchorKeyword.Anchor] = (schema, false);

		var dynamicAnchorKeyword = schema.Keywords!.OfType<DynamicAnchorKeyword>().FirstOrDefault();
		if (dynamicAnchorKeyword != null)
			resourceRoot.Anchors[dynamicAnchorKeyword.Value] = (schema, true);

		var recursiveAnchorKeyword = schema.Keywords!.OfType<RecursiveAnchorKeyword>().FirstOrDefault();
		if (recursiveAnchorKeyword is { Value: true })
			resourceRoot.RecursiveAnchor = schema;

		var subschemas = schema.Keywords!.SelectMany(GetSubschemas);

		foreach (var subschema in subschemas)
		{
			PopulateBaseUris(subschema, resourceRoot, schema.BaseUri, registry, evaluatingAs);
		}
	}

	private static IEnumerable<JsonSchema> GetSubschemas(IJsonSchemaKeyword keyword)
	{
		switch (keyword)
		{
			case ISchemaContainer { Schema: { } } container:
				yield return container.Schema;
				break;
			case ISchemaCollector collector:
				foreach (var schema in collector.Schemas)
				{
					yield return schema;
				}
				break;
			case IKeyedSchemaCollector collector:
				foreach (var schema in collector.Schemas.Values)
				{
					yield return schema;
				}
				break;
			case ICustomSchemaCollector collector:
				foreach (var schema in collector.Schemas)
				{
					yield return schema;
				}
				break;
		}
	}

	internal JsonSchema? FindSubschema(JsonPointer pointer, EvaluationOptions options)
	{
		object resolvable = this;
		for (var i = 0; i < pointer.Segments.Length; i++)
		{
			var segment = pointer.Segments[i];
			object? newResolvable = null;

			int index;
			switch (resolvable)
			{
				case ISchemaContainer container and ISchemaCollector collector:
					if (container.Schema != null)
					{
						newResolvable = container.Schema;
						i--;
					}
					else if (int.TryParse(segment.Value, out index) &&
							 index >= 0 && index < collector.Schemas.Count)
						newResolvable = collector.Schemas[index];

					break;
				case ISchemaContainer container:
					newResolvable = container.Schema;
					// need to reprocess the segment
					i--;
					break;
				case ISchemaCollector collector:
					if (int.TryParse(segment.Value, out index) &&
						index >= 0 && index < collector.Schemas.Count)
						newResolvable = collector.Schemas[index];
					break;
				case IKeyedSchemaCollector keyedCollector:
					if (keyedCollector.Schemas.TryGetValue(segment.Value, out var subschema))
						newResolvable = subschema;
					break;
				case JsonSchema { Keywords: { } } schema:
					newResolvable = schema.Keywords?.FirstOrDefault(k => k.Keyword() == segment.Value);
					break;
			}

			if (newResolvable is UnrecognizedKeyword unrecognized)
			{
				var newPointer = JsonPointer.Create(pointer.Segments.Skip(i + 1), true);
				newPointer.TryEvaluate(unrecognized.Value, out var value);
				var asSchema = FromText(value?.ToString() ?? "null");
				var hostSchema = (JsonSchema)resolvable;
				asSchema.BaseUri = hostSchema.BaseUri;
				PopulateBaseUris(asSchema, hostSchema, hostSchema.BaseUri, options.SchemaRegistry, options.EvaluatingAs);
				return asSchema;
			}

			resolvable = newResolvable!;
		}

		return resolvable as JsonSchema;
	}

	internal void UpdateBaseUri(Uri newUri)
	{
		BaseUri = newUri;
	}

	/// <summary>
	/// Implicitly converts a boolean value into one of the boolean schemas.
	/// </summary>
	/// <param name="value">The boolean value.</param>
	public static implicit operator JsonSchema(bool value)
	{
		return value ? True : False;
	}

	private string ToDebugString()
	{
		if (BoolValue.HasValue) return BoolValue.Value ? "true" : "false";
		var idKeyword = Keywords!.OfType<IdKeyword>().SingleOrDefault();
		return idKeyword?.Id.OriginalString ?? BaseUri?.OriginalString ?? "subschema";
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(JsonSchema? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;

		if (BoolValue.HasValue) return BoolValue == other.BoolValue;
		if (other.BoolValue.HasValue) return false;
		if (Keywords!.Count != other.Keywords!.Count) return false;

		if (Keywords != null)
		{
			var byKeyword = Keywords.Join(other.Keywords,
					tk => tk.Keyword(),
					ok => ok.Keyword(),
					(tk, ok) => new { ThisKeyword = tk, OtherKeyword = ok })
				.ToArray();
			if (byKeyword.Length != Keywords.Count) return false;
			if (!byKeyword.All(k => k.ThisKeyword.Equals(k.OtherKeyword))) return false;
		}

		return true;
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as JsonSchema);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = Keywords?.GetUnorderedCollectionHashCode() ?? 0;
			hashCode = (hashCode * 397) ^ BoolValue.GetHashCode();
			return hashCode;
		}
	}
}

internal class SchemaJsonConverter : JsonConverter<JsonSchema>
{
	public override JsonSchema Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.True) return JsonSchema.True;
		if (reader.TokenType == JsonTokenType.False) return JsonSchema.False;

		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("JSON Schema must be true, false, or an object");

		if (!reader.Read())
			throw new JsonException("Expected token");

		var keywords = new List<IJsonSchemaKeyword>();

		do
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Comment:
					break;
				case JsonTokenType.PropertyName:
					var keyword = reader.GetString()!;
					reader.Read();
					var keywordType = SchemaKeywordRegistry.GetImplementationType(keyword);
					if (keywordType == null)
					{
						var node = JsonSerializer.Deserialize<JsonNode>(ref reader, options);
						var unrecognizedKeyword = new UnrecognizedKeyword(keyword, node);
						keywords.Add(unrecognizedKeyword);
						break;
					}

					IJsonSchemaKeyword implementation;
					if (reader.TokenType == JsonTokenType.Null)
						implementation = SchemaKeywordRegistry.GetNullValuedKeyword(keywordType) ??
										 throw new InvalidOperationException($"No null instance registered for keyword `{keyword}`");
					else
						implementation = (IJsonSchemaKeyword)JsonSerializer.Deserialize(ref reader, keywordType, options)! ??
										 throw new InvalidOperationException($"Could not deserialize expected keyword `{keyword}`");
					keywords.Add(implementation);
					break;
				case JsonTokenType.EndObject:
					return new JsonSchema(keywords);
				default:
					throw new JsonException("Expected keyword or end of schema object");
			}
		} while (reader.Read());

		throw new JsonException("Expected token");
	}

	public override void Write(Utf8JsonWriter writer, JsonSchema value, JsonSerializerOptions options)
	{
		if (value.BoolValue == true)
		{
			writer.WriteBooleanValue(true);
			return;
		}

		if (value.BoolValue == false)
		{
			writer.WriteBooleanValue(false);
			return;
		}

		writer.WriteStartObject();
		foreach (var keyword in value.Keywords!)
		{
			JsonSerializer.Serialize(writer, keyword, keyword.GetType(), options);
		}

		writer.WriteEndObject();
	}
}

public static partial class ErrorMessages
{
	private static string? _falseSchema;

	/// <summary>
	/// Gets or sets the error message for the "false" schema.
	/// </summary>
	/// <remarks>No tokens are supported.</remarks>
	public static string FalseSchema
	{
		get => _falseSchema ?? Get();
		set => _falseSchema = value;
	}
}