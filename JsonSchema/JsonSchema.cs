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

	private class JsonPointerComparer : IComparer<JsonPointer>
	{
		public static JsonPointerComparer Instance { get; } = new();

		public int Compare(JsonPointer x, JsonPointer y)
		{
			var segmentPairs = x.Segments.Zip(y.Segments, (a, b) => (a, b));
			foreach (var pair in segmentPairs)
			{
				var segmentResult = string.Compare(pair.a.Value, pair.b.Value, StringComparison.InvariantCulture);
				if (segmentResult != 0) return segmentResult;
			}

			return x.Segments.Length.CompareTo(y.Segments.Length);
		}
	}

	private List<Requirement>? _requirements;

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

	public Uri BaseUri { get; private set; }

	public Draft DeclaredDraft { get; private set; }
	public Draft CompiledDraft { get; private set; }

	internal Dictionary<string, (JsonSchema Schema, bool IsDynamic)> Anchors { get; } = new();
	internal HashSet<JsonPointer> GeneratingRequirements { get; } = new();
	internal IJsonSchemaKeyword[] KeywordsToEvaluate { get; private set; }

	private JsonSchema(bool value)
	{
		BoolValue = value;
	}
	internal JsonSchema(IEnumerable<IJsonSchemaKeyword> keywords)
	{
		Keywords = keywords.ToArray();
		//CompileIfNeeded(Draft.Unspecified);
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
	/// Attempts to recompile as the specified draft.  If the draft can be determined
	/// by a `$schema` keyword (see <see cref="DeclaredDraft"/>), this method will have no effect.
	/// </summary>
	/// <param name="draft"></param>
	public void RecompileAs(Draft draft)
	{
		if (DeclaredDraft != Draft.Unspecified) return;

		_requirements = null;
		CompileIfNeeded(draft);
	}

	private static Uri GenerateBaseUri() => new($"https://json-everything.net/{Guid.NewGuid():N}");

	private void CompileIfNeeded(Draft desiredDraft)
	{
		if (_requirements != null) return;

		if (!BoolValue.HasValue) 
			Analyze(this, this, GenerateBaseUri(), desiredDraft, true);

		var scope = new DynamicScope(BaseUri);
		_requirements = this.GenerateRequirements(scope, JsonPointer.Empty, JsonPointer.Empty).ToList();
	}

	private static (Draft Draft, bool Declared) DetermineDraft(JsonSchema schema, Draft desiredDraft)
	{
		var schemaKeyword = (SchemaKeyword?) schema.Keywords!.FirstOrDefault(x => x is SchemaKeyword);
		if (schemaKeyword != null)
		{
			var metaSchemaId = schemaKeyword.Schema;
			while (metaSchemaId != null)
			{
				var draft = metaSchemaId.OriginalString switch
				{
					MetaSchemas.Draft6IdValue => Draft.Draft6,
					MetaSchemas.Draft7IdValue => Draft.Draft7,
					MetaSchemas.Draft201909IdValue => Draft.Draft201909,
					MetaSchemas.Draft202012IdValue => Draft.Draft202012,
					MetaSchemas.DraftNextIdValue => Draft.DraftNext,
					_ => Draft.Unspecified
				};
				if (draft != Draft.Unspecified) return (draft, true);

				var metaSchema = SchemaRegistry.Global.Get(metaSchemaId);
				if (metaSchema == null)
					throw new JsonSchemaException("Cannot resolve custom meta-schema.  Make sure meta-schemas are registered in the global registry.");

				var newMetaSchemaId = metaSchema.Keywords!.OfType<SchemaKeyword>().FirstOrDefault()?.Schema;
				if (newMetaSchemaId == metaSchemaId)
					throw new JsonSchemaException("Custom meta-schema `$schema` keywords must eventually resolve to a known draft meta-schema.");

				metaSchemaId = newMetaSchemaId;
			}
		}

		if (desiredDraft != Draft.Unspecified) return (desiredDraft, false);

		var allDraftsArray = Enum.GetValues(typeof(Draft)).Cast<Draft>().ToArray();
		var allDrafts = allDraftsArray.Aggregate(Draft.Unspecified, (a, x) => a | x);
		var commonDrafts = schema.Keywords!.Aggregate(allDrafts, (a, x) => a & x.DraftsSupported());
		var candidates = allDraftsArray.Where(x => commonDrafts.HasFlag(x)).ToArray();

		return (candidates.Any() ? candidates.Max() : Draft.DraftNext, false);
	}

	private static void Analyze(JsonSchema schema, JsonSchema resourceRoot, Uri currentBaseUri, Draft desiredDraft, bool selfRegister = false)
	{
		if (schema.BoolValue.HasValue) return;

		var (draft, declared) = DetermineDraft(schema, desiredDraft);
		schema.DeclaredDraft = declared ? draft : Draft.Unspecified;
		schema.CompiledDraft = draft;
		schema.KeywordsToEvaluate = FilterKeywords(schema.Keywords!, draft).ToArray();

		var idKeyword = schema.KeywordsToEvaluate.OfType<IdKeyword>().FirstOrDefault();

		if (idKeyword == null)
		{
			schema.BaseUri = currentBaseUri;
			if (selfRegister)
				SchemaRegistry.Global.RegisterSchema(schema.BaseUri, schema);
		}
		else
		{
			if (schema.DeclaredDraft is Draft.Draft6 or Draft.Draft7 &&
			    idKeyword.Id.OriginalString[0] == '#' &&
			    AnchorKeyword.AnchorPattern.IsMatch(idKeyword.Id.OriginalString.Substring(1)))
			{
				schema.BaseUri = currentBaseUri;
				var anchor = idKeyword.Id.OriginalString.Substring(1);
				resourceRoot.Anchors[anchor] = (schema, false);
			}
			else
			{
				resourceRoot = schema;
				schema.BaseUri = new Uri(currentBaseUri, idKeyword.Id);
				SchemaRegistry.Global.RegisterSchema(schema.BaseUri, schema);
			}
		}

		var anchorKeyword = schema.KeywordsToEvaluate.OfType<AnchorKeyword>().FirstOrDefault();
		if (anchorKeyword != null)
			resourceRoot.Anchors[anchorKeyword.Anchor] = (schema, false);

		var dynamicAnchorKeyword = schema.KeywordsToEvaluate.OfType<DynamicAnchorKeyword>().FirstOrDefault();
		if (dynamicAnchorKeyword != null)
			resourceRoot.Anchors[dynamicAnchorKeyword.Value] = (schema, true);

		var subschemas = schema.KeywordsToEvaluate.SelectMany(GetSubschemas);

		foreach (var subschema in subschemas)
		{
			Analyze(subschema, resourceRoot, schema.BaseUri, desiredDraft);
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


	private static IEnumerable<IJsonSchemaKeyword> FilterKeywords(IEnumerable<IJsonSchemaKeyword> keywords, Draft draft)
	{
		if (draft is Draft.Draft6 or Draft.Draft7)
			return DisallowSiblingRef(keywords, draft);
		return AllowSiblingRef(keywords, draft);
	}

	private static IEnumerable<IJsonSchemaKeyword> DisallowSiblingRef(IEnumerable<IJsonSchemaKeyword> keywords, Draft draft)
	{
		var refKeyword = keywords.OfType<RefKeyword>().SingleOrDefault();

		return refKeyword != null ? new[] { refKeyword } : FilterByDraft(keywords, draft);
	}

	private static IEnumerable<IJsonSchemaKeyword> AllowSiblingRef(IEnumerable<IJsonSchemaKeyword> keywords, Draft draft)
	{
		return FilterByDraft(keywords, draft);
	}

	private static IEnumerable<IJsonSchemaKeyword> FilterByDraft(IEnumerable<IJsonSchemaKeyword> keywords, Draft draft)
	{
		if (draft == Draft.Unspecified) return keywords;

		return keywords.Where(k => k.SupportsDraft(draft));
	}

	public EvaluationResults2 EvaluateCompiled(JsonNode? instance, EvaluationOptions? options = null)
	{
		CompileIfNeeded(Draft.Unspecified);

		var instanceCatalog = instance.GenerateCatalog();

		var cache = new List<KeywordResult>();
		_requirements!.Evaluate(cache, instanceCatalog, options ?? EvaluationOptions.Default);

		var localResults = cache.Where(x => x.SubschemaPath.Segments.Length == 0);

		var output = new EvaluationResults2
		{
			IsValid = localResults.All(x => x.ValidationResult != false)
		};
		// this just creates an expanded basic output currently
		output.NestedResults.AddRange(cache.GroupBy(x => x.SubschemaPath)
			.OrderBy(x => x.Key, JsonPointerComparer.Instance)
			.Select(x =>
			{
				var first = x.First();
				var result = new EvaluationResults2
				{
					EvaluationPath = x.Key,
					InstanceLocation = first.InstanceLocation,
					SchemaLocation = first.SchemaLocation,
					IsValid = x.All(k => k.ValidationResult != false)
				};
				var errors = x.Where(k => !k.ValidationResult != false && k.Error != null);
				foreach (var error in errors)
				{
					result.Errors[error.Keyword] = error.Error!;
				}

				var annotations = x.Where(k => k.ValidationResult != false && k.Annotation != null);
				foreach (var annotation in annotations)
				{
					result.Annotations[annotation.Keyword] = annotation.Annotation;
				}

				return result;
			}));
		
		return output;
	}

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
		//var baseUri = RegisterSubschemasAndGetBaseUri(options.SchemaRegistry, BaseUri ?? options.DefaultBaseUri);
		var baseUri = BaseUri;
		if (baseUri != null! && baseUri.IsAbsoluteUri)
			BaseUri = baseUri;

		var currentUri = baseUri == options.DefaultBaseUri
			? BaseUri ?? baseUri
			: baseUri;

		var context = new EvaluationContext(options, currentUri, root, this);

		context.Options.SchemaRegistry.RegisterSchema(context.CurrentUri, this);

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

	/// <summary>
	/// Registers a subschema.  To be called from <see cref="IRefResolvable"/> keywords.
	/// </summary>
	/// <param name="registry">The registry into which the subschema should be registered.</param>
	/// <param name="currentUri">The current URI.</param>
	public void RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
	{
		//RegisterSubschemasAndGetBaseUri(registry, currentUri);
	}

//	private Uri? RegisterSubschemasAndGetBaseUri(SchemaRegistry registry, Uri currentUri)
//	{
//		if (Keywords == null) return null; // boolean cases

//		var idKeyword = Keywords.OfType<IdKeyword>().SingleOrDefault();
//		var refKeyword = Keywords.OfType<RefKeyword>().SingleOrDefault();
//		var refMatters = refKeyword != null &&
//						 registry.EvaluatingAs is Draft.Draft6 or Draft.Draft7;
//		UpdateBaseUri(currentUri);
//		if (idKeyword != null && !refMatters)
//		{
//			currentUri = idKeyword.UpdateUri(currentUri);
//			var parts = idKeyword.Id.OriginalString.Split(new[] { '#' }, StringSplitOptions.None);
//			var fragment = parts.Length > 1 ? parts[1] : null;
//#pragma warning disable 8602
//			if (string.IsNullOrEmpty(fragment) || fragment[0] == '/')
//#pragma warning restore 8602
//				registry.RegisterSchema(currentUri, this);
//			else
//				registry.RegisterAnchor(currentUri, fragment, this);
//		}

//		var anchors = Keywords.OfType<IAnchorProvider>();
//		foreach (var anchor in anchors)
//		{
//			anchor.RegisterAnchor(registry, currentUri, this);
//		}

//		var keywords = Keywords.OfType<IRefResolvable>().OrderBy(k => ((IJsonSchemaKeyword)k).Priority());
//		foreach (var keyword in keywords)
//		{
//			keyword.RegisterSubschemas(registry, currentUri);
//		}

//		return currentUri;
//	}

	internal (JsonSchema?, Uri?) FindSubschema(JsonPointer pointer, Uri? currentUri)
	{
		object resolvable = this;
		for (var i = 0; i < pointer.Segments.Length; i++)
		{
			var segment = pointer.Segments[i];
			object? newResolvable = null;

			JsonSchema? subschema;
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
					if (keyedCollector.Schemas.TryGetValue(segment.Value, out subschema))
						newResolvable = subschema;
					break;
				case ICustomSchemaCollector customCollector:
					(subschema, var segmentsRead) = customCollector.FindSubschema(pointer.Segments.Skip(i).ToArray());
					newResolvable = subschema;
					i += segmentsRead - 1;
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