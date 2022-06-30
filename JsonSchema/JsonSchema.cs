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
public class JsonSchema : IRefResolvable, IEquatable<JsonSchema>
{
	/// <summary>
	/// The empty schema `{}`.  Functionally equivalent to <see cref="True"/>.
	/// </summary>
	public static readonly JsonSchema Empty = new(Enumerable.Empty<IJsonSchemaKeyword>());
	/// <summary>
	/// The `true` schema.  Passes all instances.
	/// </summary>
	public static readonly JsonSchema True = new(true);
	/// <summary>
	/// The `false` schema.  Fails all instances.
	/// </summary>
	public static readonly JsonSchema False = new(false);

	/// <summary>
	/// Gets the keywords contained in the schema.  Only populated for non-boolean schemas.
	/// </summary>
	public IReadOnlyCollection<IJsonSchemaKeyword>? Keywords { get; }

	/// <summary>
	/// For boolean schemas, gets the value.  Null if the schema isn't a boolean schema.
	/// </summary>
	public bool? BoolValue { get; }

	internal Uri? BaseUri { get; private set; }

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

	/// <summary>
	/// Validates an instance against this schema.
	/// </summary>
	/// <param name="root">The root instance.</param>
	/// <param name="options">The options to use for this validation.</param>
	/// <returns>A <see cref="ValidationResults"/> that provides the outcome of the validation.</returns>
	public ValidationResults Validate(JsonNode? root, ValidationOptions? options = null)
	{
		options = ValidationOptions.From(options ?? ValidationOptions.Default);

		options.Log.Write(() => "Registering subschemas.");
		var baseUri = RegisterSubschemasAndGetBaseUri(options.SchemaRegistry, BaseUri ?? options.DefaultBaseUri);
		if (baseUri != null! && baseUri.IsAbsoluteUri)
			BaseUri = baseUri;

		var currentUri = baseUri == options.DefaultBaseUri
			? BaseUri ?? baseUri
			: baseUri!;

		var context = new ValidationContext(options, currentUri, root, this);

		context.Options.SchemaRegistry.RegisterSchema(context.CurrentUri, this);

		options.Log.Write(() => "Beginning validation.");
		ValidateSubschema(context);

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

		options.Log.Write(() => $"Validation complete: {results.IsValid.GetValidityString()}");
		return results;
	}

	/// <summary>
	/// Registers a subschema.  To be called from <see cref="IRefResolvable"/> keywords.
	/// </summary>
	/// <param name="registry">The registry into which the subschema should be registered.</param>
	/// <param name="currentUri">The current URI.</param>
	public void RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
	{
		RegisterSubschemasAndGetBaseUri(registry, currentUri);
	}

	private Uri? RegisterSubschemasAndGetBaseUri(SchemaRegistry registry, Uri currentUri)
	{
		if (Keywords == null) return null; // boolean cases

		var idKeyword = Keywords.OfType<IdKeyword>().SingleOrDefault();
		var refKeyword = Keywords.OfType<RefKeyword>().SingleOrDefault();
		var refMatters = refKeyword != null &&
						 (registry.ValidatingAs == Draft.Draft6 || registry.ValidatingAs == Draft.Draft7);
		if (idKeyword != null && !refMatters)
		{
			currentUri = idKeyword.UpdateUri(currentUri);
			var parts = idKeyword.Id.OriginalString.Split(new[] { '#' }, StringSplitOptions.None);
			var fragment = parts.Length > 1 ? parts[1] : null;
#pragma warning disable 8602
			if (string.IsNullOrEmpty(fragment) || fragment[0] == '/')
#pragma warning restore 8602
				registry.RegisterSchema(currentUri, this);
			else
				registry.RegisterAnchor(currentUri, fragment, this);
		}

		var anchors = Keywords.OfType<IAnchorProvider>();
		foreach (var anchor in anchors)
		{
			anchor.RegisterAnchor(registry, currentUri, this);
		}

		var keywords = Keywords.OfType<IRefResolvable>().OrderBy(k => ((IJsonSchemaKeyword)k).Priority());
		foreach (var keyword in keywords)
		{
			keyword.RegisterSubschemas(registry, currentUri);
		}

		return currentUri;
	}

	/// <summary>
	/// Validates as a subschema.  To be called from within keywords.
	/// </summary>
	/// <param name="context">The validation context for this validation pass.</param>
	public void ValidateSubschema(ValidationContext context)
	{
		if (BoolValue.HasValue)
		{
			context.Log(() => $"Found {(BoolValue.Value ? "true" : "false")} schema: {BoolValue.Value.GetValidityString()}");
			if (BoolValue.Value)
				context.LocalResult.Pass();
			else
				context.LocalResult.Fail(string.Empty, ErrorMessages.FalseSchema);
			return;
		}

		var metaSchemaUri = Keywords!.OfType<SchemaKeyword>().FirstOrDefault()?.Schema;
		var keywords = context.Options.FilterKeywords(Keywords!, metaSchemaUri, context.Options.SchemaRegistry);

		List<Type>? keywordTypesToProcess = null;
		foreach (var keyword in keywords.OrderBy(k => k.Priority()))
		{
			// $schema is always processed first, and this should only be set
			// after $schema has been evaluated.
			if (keyword is not SchemaKeyword)
				keywordTypesToProcess ??= context.GetKeywordsToProcess()?.ToList();
			if (!keywordTypesToProcess?.Contains(keyword.GetType()) ?? false) continue;

			keyword.Validate(context);

			if (!context.LocalResult.IsValid && context.ApplyOptimizations) break;
		}

		if (context.IsNewDynamicScope)
			context.Options.SchemaRegistry.ExitingUriScope();
	}

	internal (JsonSchema?, Uri?) FindSubschema(JsonPointer pointer, Uri? currentUri)
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
					if (!ReferenceEquals(schema, this))
					{
						// the assumption here is that if the schema is this schema, the $ref
						// navigated directly to this schema, and we don't need to update the uri
						var idKeyword = schema.Keywords.OfType<IdKeyword>().SingleOrDefault();
						if (idKeyword != null && i != pointer.Segments.Length - 1)
							currentUri = idKeyword.UpdateUri(currentUri);
					}
					newResolvable = schema.Keywords?.FirstOrDefault(k => k.Keyword() == segment.Value);
					break;
			}

			if (newResolvable is UnrecognizedKeyword unrecognized)
			{
				var newPointer = JsonPointer.Create(pointer.Segments.Skip(i + 1), true);
				newPointer.TryEvaluate(unrecognized.Value, out var value);
				var asSchema = FromText(value?.ToString() ?? "null");
				return (asSchema, currentUri);
			}

			resolvable = newResolvable!;
		}

		return (resolvable as JsonSchema, currentUri);
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
				.ToList();
			if (byKeyword.Count != Keywords.Count) return false;
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