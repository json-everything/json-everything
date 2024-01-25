using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

[JsonSerializable(typeof(JsonSchema))]
[JsonSerializable(typeof(AdditionalItemsKeyword))]
[JsonSerializable(typeof(AdditionalPropertiesKeyword))]
[JsonSerializable(typeof(AllOfKeyword))]
[JsonSerializable(typeof(AnchorKeyword))]
[JsonSerializable(typeof(AnyOfKeyword))]
[JsonSerializable(typeof(CommentKeyword))]
[JsonSerializable(typeof(ConstKeyword))]
[JsonSerializable(typeof(ContainsKeyword))]
[JsonSerializable(typeof(ContentEncodingKeyword))]
[JsonSerializable(typeof(ContentMediaTypeKeyword))]
[JsonSerializable(typeof(ContentSchemaKeyword))]
[JsonSerializable(typeof(DefaultKeyword))]
[JsonSerializable(typeof(DefinitionsKeyword))]
[JsonSerializable(typeof(DefsKeyword))]
[JsonSerializable(typeof(DependenciesKeyword))]
[JsonSerializable(typeof(DependentRequiredKeyword))]
[JsonSerializable(typeof(DependentSchemasKeyword))]
[JsonSerializable(typeof(DeprecatedKeyword))]
[JsonSerializable(typeof(DescriptionKeyword))]
[JsonSerializable(typeof(DynamicAnchorKeyword))]
[JsonSerializable(typeof(DynamicRefKeyword))]
[JsonSerializable(typeof(ElseKeyword))]
[JsonSerializable(typeof(EnumKeyword))]
[JsonSerializable(typeof(ExamplesKeyword))]
[JsonSerializable(typeof(ExclusiveMaximumKeyword))]
[JsonSerializable(typeof(ExclusiveMinimumKeyword))]
[JsonSerializable(typeof(FormatKeyword))]
[JsonSerializable(typeof(IdKeyword))]
[JsonSerializable(typeof(IfKeyword))]
[JsonSerializable(typeof(ItemsKeyword))]
[JsonSerializable(typeof(KeywordConstraint))]
[JsonSerializable(typeof(KeywordEvaluation))]
[JsonSerializable(typeof(MaxContainsKeyword))]
[JsonSerializable(typeof(MaximumKeyword))]
[JsonSerializable(typeof(MaxItemsKeyword))]
[JsonSerializable(typeof(MaxLengthKeyword))]
[JsonSerializable(typeof(MaxPropertiesKeyword))]
[JsonSerializable(typeof(MinContainsKeyword))]
[JsonSerializable(typeof(MinimumKeyword))]
[JsonSerializable(typeof(MinItemsKeyword))]
[JsonSerializable(typeof(MinLengthKeyword))]
[JsonSerializable(typeof(MinPropertiesKeyword))]
[JsonSerializable(typeof(MultipleOfKeyword))]
[JsonSerializable(typeof(NotKeyword))]
[JsonSerializable(typeof(OneOfKeyword))]
[JsonSerializable(typeof(PatternKeyword))]
[JsonSerializable(typeof(PatternPropertiesKeyword))]
[JsonSerializable(typeof(PrefixItemsKeyword))]
[JsonSerializable(typeof(PropertiesKeyword))]
[JsonSerializable(typeof(PropertyDependenciesKeyword))]
[JsonSerializable(typeof(PropertyNamesKeyword))]
[JsonSerializable(typeof(ReadOnlyKeyword))]
[JsonSerializable(typeof(RecursiveAnchorKeyword))]
[JsonSerializable(typeof(RecursiveRefKeyword))]
[JsonSerializable(typeof(RefKeyword))]
[JsonSerializable(typeof(RequiredKeyword))]
[JsonSerializable(typeof(SchemaKeyword))]
[JsonSerializable(typeof(ThenKeyword))]
[JsonSerializable(typeof(TitleKeyword))]
[JsonSerializable(typeof(TypeKeyword))]
[JsonSerializable(typeof(UnevaluatedItemsKeyword))]
[JsonSerializable(typeof(UnevaluatedPropertiesKeyword))]
[JsonSerializable(typeof(UniqueItemsKeyword))]
[JsonSerializable(typeof(UnrecognizedKeyword))]
[JsonSerializable(typeof(VocabularyKeyword))]
[JsonSerializable(typeof(WriteOnlyKeyword))]

[JsonSerializable(typeof(SchemaValueType))]
[JsonSerializable(typeof(String[]))]
[JsonSerializable(typeof(Dictionary<string, JsonSchema>))]
[JsonSerializable(typeof(Dictionary<string, bool>))]
[JsonSerializable(typeof(List<JsonSchema>))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(JsonArray))]
[JsonSerializable(typeof(Dictionary<string, SchemaOrPropertyList>))]
[JsonSerializable(typeof(Dictionary<string, List<string>>))]
[JsonSerializable(typeof(int[]))]
[JsonSerializable(typeof(Dictionary<string, PropertyDependency>))]
internal partial class JsonSchemaSerializationContext : JsonSerializerContext
{

}

/// <summary>
/// Represents a JSON Schema.
/// </summary>
[JsonConverter(typeof(SchemaJsonConverter))]
[DebuggerDisplay("{ToDebugString()}")]
public class JsonSchema : IBaseDocument
{
	private const string _unknownKeywordsAnnotationKey = "$unknownKeywords";

#if NET6_0_OR_GREATER
	private static readonly HashSet<SpecVersion> _definedSpecVersions = [..Enum.GetValues<SpecVersion>()];
#else
	private static readonly HashSet<SpecVersion> _definedSpecVersions = [..Enum.GetValues(typeof(SpecVersion)).Cast<SpecVersion>()];
#endif

	private readonly Dictionary<string, IJsonSchemaKeyword>? _keywords;
	private readonly List<(DynamicScope Scope, SchemaConstraint Constraint)> _constraints = [];

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
	public IReadOnlyCollection<IJsonSchemaKeyword>? Keywords => _keywords?.Values;

	/// <summary>
	/// Gets the keyword class by keyword name.
	/// </summary>
	/// <param name="keyword">The keyword name.</param>
	/// <returns>The keyword implementation if it exists in the schema.</returns>
	public IJsonSchemaKeyword? this[string keyword] => _keywords?.TryGetValue(keyword, out var k) ?? false ? k : null;

	/// <summary>
	/// For boolean schemas, gets the value.  Null if the schema isn't a boolean schema.
	/// </summary>
	public bool? BoolValue { get; }

	/// <summary>
	/// Gets the base URI that applies to this schema.  This may be defined by a parent schema.
	/// </summary>
	/// <remarks>
	/// This property is initialized to a generated random value that matches `https://json-everything.net/{random}`
	/// where `random` is 10 hex characters.
	///
	/// It may change after the initial evaluation based on whether the schema contains an `$id` keyword
	/// or is a child of another schema.
	/// </remarks>
	public Uri BaseUri { get; set; } = GenerateBaseUri();

	/// <summary>
	/// Gets whether the schema defines a new schema resource.  This will only be true if it contains an `$id` keyword.
	/// </summary>
	public bool IsResourceRoot { get; private set; }

	/// <summary>
	/// Gets the specification version as determined by analyzing the `$schema` keyword, if it exists.
	/// </summary>
	public SpecVersion DeclaredVersion { get; private set; }

	internal Dictionary<string, (JsonSchema Schema, bool IsDynamic)> Anchors { get; } = [];
	internal JsonSchema? RecursiveAnchor { get; set; }

	private JsonSchema(bool value)
	{
		BoolValue = value;
	}
	internal JsonSchema(IEnumerable<IJsonSchemaKeyword> keywords)
	{
		_keywords = keywords.ToDictionary(x => x.Keyword());
	}

	/// <summary>
	/// Loads text from a file and deserializes a <see cref="JsonSchema"/>.
	/// </summary>
	/// <param name="fileName">The filename to load, URL-decoded.</param>
	/// <param name="options">Serializer options.</param>
	/// <returns>A new <see cref="JsonSchema"/>.</returns>
	/// <exception cref="JsonException">Could not deserialize a portion of the schema.</exception>
	/// <remarks>The filename needs to not be URL-encoded as <see cref="Uri"/> attempts to encode it.</remarks>
	[RequiresUnreferencedCode("TODO")]
	[RequiresDynamicCode("TODO")]
	public static JsonSchema FromFile(string fileName, JsonSerializerOptions? options)
	{
		var text = File.ReadAllText(fileName);
		var schema = FromText(text, options);
		var path = Path.GetFullPath(fileName);
		// For some reason, full *nix file paths (which start with '/') don't work quite right when
		// being prepended with 'file:///'.  It seems the '////' is interpreted as '//' and the
		// first folder in the path is then interpreted as the host.  To account for this, we
		// need to prepend with 'file://' instead.
		var protocol = path.StartsWith("/") ? "file://" : "file:///";
		schema.BaseUri = new Uri($"{protocol}{path}");
		return schema;
	}

	/// <summary>
	/// Loads text from a file and deserializes a <see cref="JsonSchema"/>.
	/// </summary>
	/// <param name="fileName">The filename to load, URL-decoded.</param>
	/// <returns>A new <see cref="JsonSchema"/>.</returns>
	/// <exception cref="JsonException">Could not deserialize a portion of the schema.</exception>
	/// <remarks>The filename needs to not be URL-encoded as <see cref="Uri"/> attempts to encode it.</remarks>
	public static JsonSchema FromFile(string fileName)
	{
		var text = File.ReadAllText(fileName);
		var schema = FromText(text);
		var path = Path.GetFullPath(fileName);
		// For some reason, full *nix file paths (which start with '/') don't work quite right when
		// being prepended with 'file:///'.  It seems the '////' is interpreted as '//' and the
		// first folder in the path is then interpreted as the host.  To account for this, we
		// need to prepend with 'file://' instead.
		var protocol = path.StartsWith("/") ? "file://" : "file:///";
		schema.BaseUri = new Uri($"{protocol}{path}");
		return schema;
	}

	/// <summary>
	/// Deserializes a <see cref="JsonSchema"/> from text.
	/// </summary>
	/// <param name="jsonText">The text to parse.</param>
	/// <param name="options">Serializer options.</param>
	/// <returns>A new <see cref="JsonSchema"/>.</returns>
	/// <exception cref="JsonException">Could not deserialize a portion of the schema.</exception>
	[RequiresUnreferencedCode("TODO")]
	[RequiresDynamicCode("TODO")]
	public static JsonSchema FromText(string jsonText, JsonSerializerOptions? options)
	{
		return JsonSerializer.Deserialize<JsonSchema>(jsonText, options)!;
	}

	/// <summary>
	/// Deserializes a <see cref="JsonSchema"/> from text.
	/// </summary>
	/// <param name="jsonText">The text to parse.</param>
	/// <returns>A new <see cref="JsonSchema"/>.</returns>
	/// <exception cref="JsonException">Could not deserialize a portion of the schema.</exception>
	public static JsonSchema FromText(string jsonText)
	{
		return JsonSerializer.Deserialize<JsonSchema>(jsonText, JsonSchemaSerializationContext.Default.JsonSchema)!;
	}

	/// <summary>
	/// Deserializes a <see cref="JsonSchema"/> from a stream.
	/// </summary>
	/// <param name="source">A stream.</param>
	/// <param name="options">Serializer options.</param>
	/// <returns>A new <see cref="JsonSchema"/>.</returns>
	[RequiresUnreferencedCode("TODO")]
	[RequiresDynamicCode("TODO")]
	public static ValueTask<JsonSchema> FromStream(Stream source, JsonSerializerOptions? options = null)
	{
		return JsonSerializer.DeserializeAsync<JsonSchema>(source, options)!;
	}

	/// <summary>
	/// Deserializes a <see cref="JsonSchema"/> from a stream.
	/// </summary>
	/// <param name="source">A stream.</param>
	/// <returns>A new <see cref="JsonSchema"/>.</returns>
	[RequiresUnreferencedCode("TODO")]
	[RequiresDynamicCode("TODO")]
	public static ValueTask<JsonSchema> FromStream(Stream source)
	{
		return JsonSerializer.DeserializeAsync<JsonSchema>(source, JsonSchemaSerializationContext.Default.JsonSchema)!;
	}

	/// <summary>
	/// Evaluates an instance by automatically determining the schema to use by examining
	/// the instance's `$schema` key.
	/// </summary>
	/// <param name="root">The root instance.</param>
	/// <param name="options">The options to use for this evaluation.</param>
	/// <returns>A <see cref="EvaluationResults"/> that provides the outcome of the evaluation.</returns>
	/// <exception cref="ArgumentException">
	/// Throw when the instance doesn't have a `$schema` key, when the value under `$schema` is not
	/// an absolute URI, or when the URI is not associated with a registered schema.
	/// </exception>
	// TODO: Not quite ready to release this.  Is it a good practice?  https://github.com/orgs/json-schema-org/discussions/473
	internal static EvaluationResults AutoEvaluate(JsonNode? root, EvaluationOptions? options = null)
	{
		string? schemaId = null;
		(root as JsonObject)?[SchemaKeyword.Name]?.AsValue().TryGetValue(out schemaId);
		if (schemaId == null || !Uri.TryCreate(schemaId, UriKind.Absolute, out var schemaUri))
			throw new ArgumentException("JSON must contain `$schema` with an absolute URI.", nameof(root));

		options ??= EvaluationOptions.Default;

		var schema = options.SchemaRegistry.Get(schemaUri) as JsonSchema ??
		             throw new ArgumentException($"Schema URI {schemaId} unrecognized", nameof(root));

		return schema.Evaluate(root, options);
	}

	private static Uri GenerateBaseUri() => new($"https://json-everything.net/{Guid.NewGuid().ToString("N")[..10]}");

	/// <summary>
	/// Gets a specified keyword if it exists.
	/// </summary>
	/// <typeparam name="T">The type of the keyword to get.</typeparam>
	/// <returns>The keyword if it exists; otherwise null.</returns>
	public T? GetKeyword<T>()
		where T : IJsonSchemaKeyword
	{
		var keyword = typeof(T).Keyword();
		return (T?)this[keyword];
	}

	/// <summary>
	/// Gets a specified keyword if it exists.
	/// </summary>
	/// <param name="keyword">The keyword if it exists; otherwise null.</param>
	/// <typeparam name="T">The type of the keyword to get.</typeparam>
	/// <returns>true if the keyword exists; otherwise false.</returns>
	public bool TryGetKeyword<T>(out T? keyword)
		where T : IJsonSchemaKeyword
	{
		var name = typeof(T).Keyword();
		return TryGetKeyword(name, out keyword);

	}

	/// <summary>
	/// Gets a specified keyword if it exists.
	/// </summary>
	/// <typeparam name="T">The type of the keyword to get.</typeparam>
	/// <param name="keywordName">The name of the keyword.</param>
	/// <param name="keyword">The keyword if it exists; otherwise null.</param>
	/// <returns>true if the keyword exists; otherwise false.</returns>
	public bool TryGetKeyword<T>(string keywordName, out T? keyword)
		where T : IJsonSchemaKeyword
	{
		if (BoolValue.HasValue)
		{
			keyword = default;
			return false;
		}

		if (_keywords!.TryGetValue(keywordName, out var k))
		{
			keyword = (T)k!;
			return true;
		}

		keyword = default;
		return false;
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

		// BaseUri may change if $id is present
		// TODO: remove options.EvaluatingAs
		var evaluatingAs = DetermineSpecVersion(this, options.SchemaRegistry, options.EvaluateAs);
		PopulateBaseUris(this, this, BaseUri, options.SchemaRegistry, evaluatingAs, true);


		var context = new EvaluationContext(options, evaluatingAs, BaseUri);
		var constraint = BuildConstraint(JsonPointer.Empty, JsonPointer.Empty, JsonPointer.Empty, context.Scope);
		if (!BoolValue.HasValue)
			PopulateConstraint(constraint, context);

		var evaluation = constraint.BuildEvaluation(root, JsonPointer.Empty, JsonPointer.Empty, options);
		evaluation.Evaluate(context);

		if (options.AddAnnotationForUnknownKeywords && constraint.UnknownKeywords != null)
			evaluation.Results.SetAnnotation(_unknownKeywordsAnnotationKey, constraint.UnknownKeywords);

		var results = evaluation.Results;
		switch (options.OutputFormat)
		{
			case OutputFormat.Flag:
				results.ToFlag();
				break;
			case OutputFormat.List:
				results.ToList();
				break;
			case OutputFormat.Hierarchical:
				break;
			default:
				throw new ArgumentOutOfRangeException("options.OutputFormat");
		}

		return results;
	}

	private bool IsDynamic()
	{
		if (BoolValue.HasValue) return false;
		if (Keywords!.Any(x => x is DynamicRefKeyword or RecursiveRefKeyword)) return true;

		return Keywords!.SelectMany(GetSubschemas).Any(x => x.IsDynamic());
	}

	/// <summary>
	/// Builds a constraint for the schema.
	/// </summary>
	/// <param name="relativeEvaluationPath">
	/// The relative evaluation path in JSON Pointer form.  Generally this will be a keyword name,
	/// but may have other segments, such as in the case of `properties` which also has the property name.
	/// </param>
	/// <param name="baseInstanceLocation">The base location within the instance that is being evaluated.</param>
	/// <param name="relativeInstanceLocation">
	/// The location relative to <paramref name="baseInstanceLocation"/> within the instance that
	/// is being evaluated.
	/// </param>
	/// <param name="context">The evaluation context.</param>
	/// <returns>A schema constraint.</returns>
	/// <remarks>
	/// The constraint returned by this method is cached by the <see cref="JsonSchema"/> object.
	/// Different evaluation paths to this schema object may result in different constraints, so
	/// a new constraint is saved for each dynamic scope.
	/// </remarks>
	public SchemaConstraint GetConstraint(JsonPointer relativeEvaluationPath, JsonPointer baseInstanceLocation, JsonPointer relativeInstanceLocation, EvaluationContext context)
	{
		var baseUri = BoolValue.HasValue ? context.Scope.LocalScope : BaseUri;
	
		var scopedConstraint = CheckScopedConstraints(context.Scope);
		if (scopedConstraint != null)
			return new SchemaConstraint(relativeEvaluationPath, baseInstanceLocation.Combine(relativeInstanceLocation), relativeInstanceLocation, baseUri, this)
			{
				Source = scopedConstraint
			};

		var constraint = BuildConstraint(relativeEvaluationPath, baseInstanceLocation, relativeInstanceLocation, context.Scope);
		if (!BoolValue.HasValue) 
			PopulateConstraint(constraint, context);

		return constraint;
	}

	private SchemaConstraint BuildConstraint(JsonPointer evaluationPath, JsonPointer baseInstanceLocation, JsonPointer relativeInstanceLocation, DynamicScope scope)
	{
		lock (_constraints)
		{
			var scopedConstraint = CheckScopedConstraints(scope);
			if (scopedConstraint != null) return scopedConstraint;

			var baseUri = BoolValue.HasValue ? scope.LocalScope : BaseUri;

			var constraint = new SchemaConstraint(evaluationPath, baseInstanceLocation.Combine(relativeInstanceLocation), relativeInstanceLocation, baseUri, this);
			_constraints.Add((new DynamicScope(scope), constraint));
		
			return constraint;
		}
	}

	private SchemaConstraint? CheckScopedConstraints(DynamicScope scope)
	{
		SchemaConstraint? scopedConstraint;
		// ReSharper disable InconsistentlySynchronizedField
		// We only need to worry about synchronization when potentially adding new constraints
		// which only happens in BuildConstraint().
		if (IsDynamic())
			(_, scopedConstraint) = _constraints.FirstOrDefault(x => x.Scope.Equals(scope));
		else
			scopedConstraint = _constraints.SingleOrDefault().Constraint;
		// ReSharper restore InconsistentlySynchronizedField
		return scopedConstraint;
	}

	private void PopulateConstraint(SchemaConstraint constraint, EvaluationContext context)
	{
		if (constraint.Constraints.Length != 0) return;
		lock (constraint)
		{
			if (constraint.Constraints.Length != 0) return;

			if (context.EvaluatingAs is SpecVersion.Draft6 or SpecVersion.Draft7)
			{
				// base URI doesn't change for $ref schemas in draft 6/7
				var refKeyword = (RefKeyword?) Keywords!.FirstOrDefault(x => x is RefKeyword);
				if (refKeyword != null)
				{
					var refConstraint = refKeyword.GetConstraint(constraint, Array.Empty<KeywordConstraint>(), context);
					constraint.Constraints = [refConstraint];
					return;
				}
			}

			var dynamicScopeChanged = false;
			if (context.Scope.LocalScope != BaseUri)
			{
				dynamicScopeChanged = true;
				context.Scope.Push(BaseUri);
				context.PushEvaluatingAs(DeclaredVersion);
			}
			var localConstraints = new List<KeywordConstraint>();
			var version = DeclaredVersion == SpecVersion.Unspecified ? context.EvaluatingAs : DeclaredVersion;
			var keywords = EvaluationOptions.FilterKeywords(context.GetKeywordsToProcess(this, context.Options), version).ToArray();
			var unrecognized = Keywords!.OfType<UnrecognizedKeyword>();
			var unrecognizedButSupported = Keywords!.Except(keywords).ToArray();
			if (context.Options.AddAnnotationForUnknownKeywords)
				constraint.UnknownKeywords = new JsonArray(unrecognizedButSupported.Concat(unrecognized)
					.Select(x => (JsonNode?)x.Keyword())
					.ToArray());
			foreach (var keyword in keywords.OrderBy(x => x.Priority()))
			{
				var keywordConstraint = keyword.GetConstraint(constraint, localConstraints, context);
				localConstraints.Add(keywordConstraint);
			}

			foreach (var keyword in unrecognizedButSupported)
			{
				var jsonText = JsonSerializer.Serialize((object) keyword);
				var json = JsonNode.Parse(jsonText);
				var keywordConstraint = KeywordConstraint.SimpleAnnotation(keyword.Keyword(), json);
				localConstraints.Add(keywordConstraint);
			}

			constraint.Constraints = [.. localConstraints];
			if (dynamicScopeChanged)
			{
				context.Scope.Pop();
				context.PopEvaluatingAs();
			}
		}
	}

	internal static void Initialize(JsonSchema schema, SchemaRegistry registry, Uri? baseUri = null)
	{
		PopulateBaseUris(schema, schema, baseUri ?? schema.BaseUri, registry, selfRegister: true);
	}

	private static SpecVersion DetermineSpecVersion(JsonSchema schema, SchemaRegistry registry, SpecVersion desiredDraft)
	{
		if (schema.BoolValue.HasValue) return SpecVersion.DraftNext;
		if (schema.DeclaredVersion != SpecVersion.Unspecified) return schema.DeclaredVersion;
		if (!_definedSpecVersions.Contains(desiredDraft)) return desiredDraft;

		if (schema.TryGetKeyword<SchemaKeyword>(SchemaKeyword.Name, out var schemaKeyword))
		{
			var metaSchemaId = schemaKeyword?.Schema;
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

				var metaSchema = registry.Get(metaSchemaId) as JsonSchema ??
					throw new JsonSchemaException("Cannot resolve custom meta-schema.");

				if (metaSchema.TryGetKeyword<SchemaKeyword>(SchemaKeyword.Name, out var newMetaSchemaKeyword) &&
				    newMetaSchemaKeyword!.Schema == metaSchemaId)
					throw new JsonSchemaException("Custom meta-schema `$schema` keywords must eventually resolve to a meta-schema for a supported specification version.");

				metaSchemaId = newMetaSchemaKeyword!.Schema;
			}
		}

		if (desiredDraft != SpecVersion.Unspecified) return desiredDraft;

#if NET6_0_OR_GREATER
		var allDraftsArray = Enum.GetValues<SpecVersion>();
#else
		var allDraftsArray = Enum.GetValues(typeof(SpecVersion)).Cast<SpecVersion>().ToArray();
#endif
		var allDrafts = allDraftsArray.Aggregate(SpecVersion.Unspecified, (a, x) => a | x);
		var commonDrafts = schema.Keywords!.Aggregate(allDrafts, (a, x) => a & x.VersionsSupported());
		var candidates = allDraftsArray.Where(x => commonDrafts.HasFlag(x)).ToArray();

		return candidates.Length != 0 ? candidates.Max() : SpecVersion.DraftNext;
	}

	private static void PopulateBaseUris(JsonSchema schema, JsonSchema resourceRoot, Uri currentBaseUri, SchemaRegistry registry, SpecVersion evaluatingAs = SpecVersion.Unspecified, bool selfRegister = false)
	{
		if (schema.BoolValue.HasValue) return;
		evaluatingAs = DetermineSpecVersion(schema, registry, evaluatingAs);
		if (evaluatingAs is SpecVersion.Draft6 or SpecVersion.Draft7 &&
			schema.TryGetKeyword<RefKeyword>(RefKeyword.Name, out _))
		{
			schema.BaseUri = currentBaseUri;
			if (selfRegister)
				registry.RegisterSchema(schema.BaseUri, schema);
		}
		else
		{
			var idKeyword = (IIdKeyword?)schema.Keywords!.FirstOrDefault(x => x is IIdKeyword);
			if (idKeyword != null)
			{
				if (evaluatingAs <= SpecVersion.Draft7 &&
				    idKeyword.Id.OriginalString[0] == '#' &&
				    AnchorKeyword.AnchorPattern.IsMatch(idKeyword.Id.OriginalString[1..]))
				{
					schema.BaseUri = currentBaseUri;
					resourceRoot.Anchors[idKeyword.Id.OriginalString[1..]] = (schema, false);
				}
				else
				{
					schema.IsResourceRoot = true;
					schema.DeclaredVersion = evaluatingAs;
					resourceRoot = schema;
					schema.BaseUri = new Uri(currentBaseUri, idKeyword.Id);
					registry.RegisterSchema(schema.BaseUri, schema);
				}
			}
			else
			{
				schema.BaseUri = currentBaseUri;
				if (selfRegister)
					registry.RegisterSchema(schema.BaseUri, schema);
			}

			if (schema.TryGetKeyword<AnchorKeyword>(AnchorKeyword.Name, out var anchorKeyword))
			{
				resourceRoot.Anchors[anchorKeyword!.Anchor] = (schema, false);
			}

			if (schema.TryGetKeyword<DynamicAnchorKeyword>(DynamicAnchorKeyword.Name, out var dynamicAnchorKeyword))
			{
				resourceRoot.Anchors[dynamicAnchorKeyword!.Value] = (schema, true);
			}

			schema.TryGetKeyword<RecursiveAnchorKeyword>(RecursiveAnchorKeyword.Name, out var recursiveAnchorKeyword);
			if (recursiveAnchorKeyword is { Value: true })
				resourceRoot.RecursiveAnchor = schema;
		}

		var subschemas = schema.Keywords!.SelectMany(GetSubschemas);

		foreach (var subschema in subschemas)
		{
			PopulateBaseUris(subschema, resourceRoot, schema.BaseUri, registry, evaluatingAs);
		}
	}

	internal static IEnumerable<JsonSchema> GetSubschemas(IJsonSchemaKeyword keyword)
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

	JsonSchema? IBaseDocument.FindSubschema(JsonPointer pointer, EvaluationOptions options)
	{
		object? ExtractSchemaFromData(JsonPointer localPointer, JsonNode? data, JsonSchema hostSchema)
		{
			if (!localPointer.TryEvaluate(data, out var value)) return null;

			var asSchema = FromText(value?.ToString() ?? "null");
			asSchema.BaseUri = hostSchema.BaseUri;
			PopulateBaseUris(asSchema, hostSchema, hostSchema.BaseUri, options.SchemaRegistry);
			return asSchema;
		}

		object? CheckResolvable(object localResolvable, ref int i, string pointerSegment, ref JsonSchema hostSchema)
		{
			int index;
			object? newResolvable = null;
			switch (localResolvable)
			{
				case ISchemaContainer container and ISchemaCollector collector:
					if (container.Schema != null!)
					{
						hostSchema = container.Schema;
						newResolvable = hostSchema;
						i--;
					}
					else if (int.TryParse(pointerSegment, out index) &&
					         index >= 0 && index < collector.Schemas.Count)
					{
						hostSchema = collector.Schemas[index];
						newResolvable = hostSchema;
					}
					break;
				case ISchemaContainer container:
					newResolvable = container.Schema;
					// need to reprocess the segment
					i--;
					break;
				case ISchemaCollector collector:
					if (int.TryParse(pointerSegment, out index) &&
					    index >= 0 && index < collector.Schemas.Count)
					{
						hostSchema = collector.Schemas[index];
						newResolvable = hostSchema;
					}
					break;
				case IKeyedSchemaCollector keyedCollector:
					if (keyedCollector.Schemas.TryGetValue(pointerSegment, out var subschema))
					{
						hostSchema = subschema;
						newResolvable = hostSchema;
					}
					break;
				case ICustomSchemaCollector customCollector:
					var (found, segmentsConsumed) = customCollector.FindSubschema(pointer.Segments.Skip(i).ToReadOnlyList());
					hostSchema = found!;
					newResolvable = hostSchema;
					i += segmentsConsumed;
					break;
				case JsonSchema { _keywords: not null } schema:
					schema._keywords.TryGetValue(pointerSegment, out var k);
					newResolvable = k;
					break;
				default: // non-applicator keyword
					var serialized = JsonSerializer.Serialize(localResolvable);
					var json = JsonNode.Parse(serialized);
					var newPointer = JsonPointer.Create(pointer.Segments.Skip(i));
					i += newPointer.Segments.Length - 1;
					return ExtractSchemaFromData(newPointer, json, hostSchema);
			}

			if (newResolvable is UnrecognizedKeyword unrecognized)
			{
				var newPointer = JsonPointer.Create(pointer.Segments.Skip(i + 1));
				i += newPointer.Segments.Length;
				return ExtractSchemaFromData(newPointer, unrecognized.Value, (JsonSchema)localResolvable);
			}

			return newResolvable;
		}

		object? resolvable = this;
		var currentSchema = this;
		for (var i = 0; i < pointer.Segments.Length; i++)
		{
			var segment = pointer.Segments[i];

			resolvable = CheckResolvable(resolvable, ref i, segment.Value, ref currentSchema);
			if (resolvable == null) return null;
		}

		if (resolvable is JsonSchema target) return target;

		var count = pointer.Segments.Length;
		// These parameters don't really matter.  This extra check only captures the case where the
		// last segment of the pointer is an ISchemaContainer.
		return CheckResolvable(resolvable, ref count, null!, ref currentSchema) as JsonSchema;
	}

	/// <summary>
	/// Gets a defined anchor.
	/// </summary>
	/// <param name="anchorName">The name of the anchor (excluding the `#`)</param>
	/// <returns>The associated subschema, if the anchor exists, or null.</returns>
	public JsonSchema? GetAnchor(string anchorName) =>
		Anchors.TryGetValue(anchorName, out var anchorDefinition)
			? anchorDefinition.IsDynamic
				? null
				: anchorDefinition.Schema
			: null;

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
		var idKeyword = Keywords!.OfType<IIdKeyword>().SingleOrDefault();
		return idKeyword?.Id.OriginalString ?? BaseUri.OriginalString;
	}
}

/// <summary>
/// JSON converter for <see cref="JsonSchema"/>.
/// </summary>
public sealed class SchemaJsonConverter : JsonConverter<JsonSchema>
{
	/// <summary>Reads and converts the JSON to type <see cref="JsonSchema"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
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
						var node = JsonSerializer.Deserialize(ref reader, JsonSchemaSerializationContext.Default.JsonNode)!;
						var unrecognizedKeyword = new UnrecognizedKeyword(keyword, node);
						keywords.Add(unrecognizedKeyword);
						break;
					}

					IJsonSchemaKeyword implementation;
					if (reader.TokenType == JsonTokenType.Null)
						implementation = SchemaKeywordRegistry.GetNullValuedKeyword(keywordType) ??
										 throw new InvalidOperationException($"No null instance registered for keyword `{keyword}`");
					else
					{
#if NET6_0_OR_GREATER
						if (SchemaKeywordRegistry.TryGetTypeInfo(keywordType, out JsonTypeInfo? typeinfo))
						{
							implementation = JsonSerializer.Deserialize(ref reader, typeinfo!) as IJsonSchemaKeyword ??
								throw new InvalidOperationException($"Could not deserialize expected keyword `{keyword}`");
						}
						else
#endif
						if (SchemaKeywordRegistry.RequiresDynamicSerialization)
						{
#pragma warning disable IL2026, IL3050 // Suppress because to get here the caller of SchemaKeywordRegistry must have suppressed the AOT warnings themselves
							implementation = JsonSerializer.Deserialize(ref reader, keywordType) as IJsonSchemaKeyword ??
								throw new InvalidOperationException($"Could not deserialize expected keyword `{keyword}`");
#pragma warning restore IL2026, IL3050
						}
						else
						{
							throw new InvalidOperationException($"Could not deserialize keyword `{keyword}`; missing type information");
						}
					}
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

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
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
			writer.WritePropertyName(keyword.Keyword());
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