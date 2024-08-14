using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Json.More;
using Json.Pointer;
// ReSharper disable LocalizableElement

namespace Json.Schema;

/// <summary>
/// Represents a JSON Schema.
/// </summary>
[JsonConverter(typeof(SchemaJsonConverter))]
[DebuggerDisplay("{ToDebugString()}")]
public class JsonSchema : IBaseDocument
{
	private const string _unknownKeywordsAnnotationKey = "$unknownKeywords";

	private readonly Dictionary<string, IJsonSchemaKeyword>? _keywords;
	// using ConcurrentStack because it has a Clear() method
	private readonly ConcurrentStack<(DynamicScope Scope, SchemaConstraint Constraint)> _constraints = [];

	private EvaluationOptions? _lastCalledOptions;
	private bool? _isDynamic;

	/// <summary>
	/// The empty schema `{}`.  Functionally equivalent to <see cref="True"/>.
	/// </summary>
	public static readonly JsonSchema Empty = new([]);
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
	public SpecVersion DeclaredVersion { get; internal set; }

	internal Vocabulary[]? Dialect { get; set; }

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
	[RequiresUnreferencedCode("Calls JsonSerializer.Deserialize with JsonSerializerOptions. Make sure the options object contains all relevant JsonTypeInfos before suppressing this warning.")]
	[RequiresDynamicCode("Calls JsonSerializer.Deserialize with JsonSerializerOptions. Make sure the options object contains all relevant JsonTypeInfos before suppressing this warning.")]
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
	[RequiresUnreferencedCode("Calls JsonSerializer.Deserialize with JsonSerializerOptions. Make sure the options object contains all relevant JsonTypeInfos before suppressing this warning.")]
	[RequiresDynamicCode("Calls JsonSerializer.Deserialize with JsonSerializerOptions. Make sure the options object contains all relevant JsonTypeInfos before suppressing this warning.")]
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
		return JsonSerializer.Deserialize(jsonText, JsonSchemaSerializerContext.Default.JsonSchema)!;
	}

	/// <summary>
	/// Deserializes a <see cref="JsonSchema"/> from a stream.
	/// </summary>
	/// <param name="source">A stream.</param>
	/// <param name="options">Serializer options.</param>
	/// <returns>A new <see cref="JsonSchema"/>.</returns>
	[RequiresUnreferencedCode("Calls JsonSerializer.Deserialize with JsonSerializerOptions. Make sure the options object contains all relevant JsonTypeInfos before suppressing this warning.")]
	[RequiresDynamicCode("Calls JsonSerializer.Deserialize with JsonSerializerOptions. Make sure the options object contains all relevant JsonTypeInfos before suppressing this warning.")]
	public static ValueTask<JsonSchema> FromStream(Stream source, JsonSerializerOptions? options)
	{
		return JsonSerializer.DeserializeAsync<JsonSchema>(source, options)!;
	}

	/// <summary>
	/// Deserializes a <see cref="JsonSchema"/> from a stream.
	/// </summary>
	/// <param name="source">A stream.</param>
	/// <returns>A new <see cref="JsonSchema"/>.</returns>
	public static ValueTask<JsonSchema> FromStream(Stream source)
	{
		return JsonSerializer.DeserializeAsync(source, JsonSchemaSerializerContext.Default.JsonSchema)!;
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
	public bool TryGetKeyword<T>([NotNullWhen(true)] out T? keyword)
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
	public bool TryGetKeyword<T>(string keywordName, [NotNullWhen(true)] out T? keyword)
		where T : IJsonSchemaKeyword
	{
		if (BoolValue.HasValue)
		{
			keyword = default;
			return false;
		}

		if (_keywords!.TryGetValue(keywordName, out var k))
		{
			keyword = (T)k;
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
		options ??= EvaluationOptions.Default;
		if (!ReferenceEquals(options, _lastCalledOptions) || options.Changed)
			ClearConstraints();
		_lastCalledOptions = options;
		options.Changed = false;

		options = EvaluationOptions.From(options);
		options.SchemaRegistry.Register(this);

		var context = new EvaluationContext(options, DeclaredVersion, BaseUri);
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

	private void ClearConstraints()
	{
		if (_subschemas is null)
		{
			using var owner = MemoryPool<JsonSchema>.Shared.Rent(CountSubschemas());
			_ = GetSubschemas(owner);
			if (_subschemas is null) return;
		}

		foreach (var subschema in _subschemas!)
		{
			subschema.ClearConstraints();
		}
		_constraints.Clear();
	}

	private bool IsDynamic()
	{
		if (BoolValue.HasValue) return false;
		if (_isDynamic.HasValue) return _isDynamic.Value;

		foreach (var keyword in Keywords!)
		{
			if (keyword is DynamicRefKeyword or RecursiveRefKeyword)
			{
				_isDynamic = true;
				return true;
			}
		}

		foreach (var subschema in _subschemas!)
		{
			if (subschema.IsDynamic())
			{
				_isDynamic = true;
				return true;
			}
		}

		_isDynamic = false;
		return false;
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
			_constraints.Push((new DynamicScope(scope), constraint));
		
			return constraint;
		}
	}

	private SchemaConstraint? CheckScopedConstraints(DynamicScope scope)
	{
		SchemaConstraint? scopedConstraint;
		if (IsDynamic())
			(_, scopedConstraint) = _constraints.FirstOrDefault(x => x.Scope.Equals(scope));
		else
			scopedConstraint = _constraints.SingleOrDefault().Constraint;
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
					context.PushEvaluationPath(RefKeyword.Name);
					var refConstraint = refKeyword.GetConstraint(constraint, [], context);  // indirect allocation
					context.PopEvaluationPath();
					constraint.Constraints = [refConstraint];  // allocation
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

			using var constraintOwner = MemoryPool<KeywordConstraint>.Shared.Rent(Keywords!.Count);
			var localConstraints = constraintOwner.Memory.Span;
			var constraintCount = 0;

			var version = DeclaredVersion == SpecVersion.Unspecified ? context.EvaluatingAs : DeclaredVersion;
			if (context.Options.AddAnnotationForUnknownKeywords) 
				constraint.UnknownKeywords = [];  // allocation;

			using var dialectOwner = MemoryPool<Type>.Shared.Rent();
			var declaredKeywordTypes = dialectOwner.Memory.Span;
			var i = 0;
			if (Dialect is not null)
			{
				foreach (var vocabulary in Dialect)
				{
					foreach (var keywordType in vocabulary.Keywords)
					{
						declaredKeywordTypes[i] = keywordType;
						i++;
					}
				}
			}

			declaredKeywordTypes = declaredKeywordTypes[..i];

			foreach (var keyword in Keywords.OrderBy(x => x.Priority()))
			{
				KeywordConstraint? keywordConstraint;
				if (ShouldProcessKeyword(keyword, context.Options.ProcessCustomKeywords, version, declaredKeywordTypes))
				{
					context.PushEvaluationPath(keyword.Keyword());
					keywordConstraint = keyword.GetConstraint(constraint, localConstraints[..constraintCount], context);  // indirect allocation
					context.PopEvaluationPath();
					localConstraints[constraintCount] = keywordConstraint;
					constraintCount++;

					if (keyword is UnrecognizedKeyword unrecognized) 
						constraint.UnknownKeywords?.Add((JsonNode)unrecognized.Name);  // allocation

					continue;
				}

				var typeInfo = SchemaKeywordRegistry.GetTypeInfo(keyword.GetType());
				var json = JsonSerializer.SerializeToNode(keyword, typeInfo!);  // indirect allocation
				keywordConstraint = KeywordConstraint.SimpleAnnotation(keyword.Keyword(), json);
				localConstraints[constraintCount] = keywordConstraint;
				constraintCount++;

				constraint.UnknownKeywords?.Add((JsonNode)keyword.Keyword());  // allocation
			}

			constraint.Constraints = localConstraints[..constraintCount].ToArray();  // allocation
			if (dynamicScopeChanged)
			{
				context.Scope.Pop();
				context.PopEvaluatingAs();
			}
		}
	}

	private bool ShouldProcessKeyword(IJsonSchemaKeyword keyword, bool processCustomKeywords, SpecVersion preferredVersion, ReadOnlySpan<Type> declaredKeywordTypes)
	{
		if (!processCustomKeywords && Dialect is not null)
		{
			var found = false;
			foreach (var type in declaredKeywordTypes)
			{
				if (type != keyword.GetType()) continue;

				found = true;
				break;
			}

			if (!found) return false;
		}

		if (!Enum.IsDefined(typeof(SpecVersion), preferredVersion) || preferredVersion == SpecVersion.Unspecified) return true;

		return keyword.SupportsVersion(preferredVersion);
	}

	private JsonSchema[]? _subschemas;

	internal int CountSubschemas()
	{
		if (BoolValue.HasValue) return 0;
		if (_subschemas is not null) return _subschemas.Length;

		return Keywords!.Sum(CountSubschemas);
	}

	private static int CountSubschemas(IJsonSchemaKeyword keyword)
	{
		return keyword switch
		{
			// ReSharper disable once RedundantAlwaysMatchSubpattern
			ISchemaContainer { Schema: not null } container => 1 + container.Schema.CountSubschemas(),
			ISchemaCollector collector => collector.Schemas.Count + collector.Schemas.Sum(x => x.CountSubschemas()),
			IKeyedSchemaCollector collector => collector.Schemas.Count + collector.Schemas.Values.Sum(x => x.CountSubschemas()),
			ICustomSchemaCollector collector => collector.Schemas.Count() + collector.Schemas.Sum(x => x.CountSubschemas()),
			_ => 0
		};
	}

	internal ReadOnlySpan<JsonSchema> GetSubschemas(IMemoryOwner<JsonSchema> owner)
	{
		if (BoolValue.HasValue) return [];
		if (_subschemas is not null) return _subschemas;

		var span = owner.Memory.Span;

		using var keywordOwner = MemoryPool<JsonSchema>.Shared.Rent(100000);
		var i = 0;
		foreach (var keyword in Keywords!)
		{
			foreach (var subschema in GetSubschemas(keyword, keywordOwner))
			{
				span[i] = subschema;
				i++;
			}
		}

		_subschemas = i == 0 ? [] : span[..i].ToArray();

		return i == 0 ? [] : span[..i];
	}

	private static ReadOnlySpan<JsonSchema> GetSubschemas(IJsonSchemaKeyword keyword, IMemoryOwner<JsonSchema> owner)
	{
		var span = owner.Memory.Span;

		int i = 0;
		switch (keyword)
		{
			// ReSharper disable once RedundantAlwaysMatchSubpattern
			case ISchemaContainer { Schema: not null } container:
				span[0] = container.Schema;
				i++;
				break;
			case ISchemaCollector collector:
				foreach (var schema in collector.Schemas)
				{
					span[i] = schema;
					i++;
				}
				break;
			case IKeyedSchemaCollector collector:
				foreach (var schema in collector.Schemas.Values)
				{
					span[i] = schema;
					i++;
				}
				break;
			case ICustomSchemaCollector collector:
				foreach (var schema in collector.Schemas)
				{
					span[i] = schema;
					i++;
				}
				break;
		}

		return i == 0 ? [] : span[..i];
	}

	JsonSchema? IBaseDocument.FindSubschema(JsonPointer pointer, EvaluationOptions options)
	{
		object? ExtractSchemaFromData(JsonPointer localPointer, JsonNode? data, JsonSchema hostSchema)
		{
			if (!localPointer.TryEvaluate(data, out var value)) return null;

			var asSchema = FromText(value?.ToString() ?? "null");
			asSchema.BaseUri = hostSchema.BaseUri;
			options.SchemaRegistry.Initialize(hostSchema.BaseUri, asSchema);
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
#if NETSTANDARD2_0
					var (found, segmentsConsumed) = customCollector.FindSubschema(pointer.GetLocal(i));
#else
					var (found, segmentsConsumed) = customCollector.FindSubschema(pointer[i..]);
#endif
					hostSchema = found!;
					newResolvable = hostSchema;
					i += segmentsConsumed;
					break;
				case JsonSchema { _keywords: not null } schema:
					schema._keywords.TryGetValue(pointerSegment, out var k);
					newResolvable = k;
					break;
				default: // non-applicator keyword
					var typeInfo = SchemaKeywordRegistry.GetTypeInfo(localResolvable.GetType());
					var serialized = JsonSerializer.Serialize(localResolvable, typeInfo!);
					var json = JsonNode.Parse(serialized);
#if NETSTANDARD2_0
					var newPointer = pointer.GetLocal(i);
#else
					var newPointer = pointer[i..];
#endif
					i += newPointer.Count - 1;
					return ExtractSchemaFromData(newPointer, json, hostSchema);
			}

			if (newResolvable is UnrecognizedKeyword unrecognized)
			{
				if (!options.AllowReferencesIntoUnknownKeywords)
					throw new InvalidOperationException($"Encountered reference into unknown keyword: {BaseUri}#{pointer}");

#if NETSTANDARD2_0
				var newPointer = pointer.GetLocal(i+1);
#else
				var newPointer = pointer[(i+1)..];
#endif
				i += newPointer.Count;
				return ExtractSchemaFromData(newPointer, unrecognized.Value, (JsonSchema)localResolvable);
			}

			return newResolvable;
		}

		object? resolvable = this;
		var currentSchema = this;
		for (var i = 0; i < pointer.Count; i++)
		{
			var segment = pointer[i];

			resolvable = CheckResolvable(resolvable, ref i, segment, ref currentSchema);
			if (resolvable == null) return null;
		}

		if (resolvable is JsonSchema target) return target;

		var count = pointer.Count;
		// These parameters don't really matter.  This extra check only captures the case where the
		// last segment of the pointer is an ISchemaContainer.
		return CheckResolvable(resolvable, ref count, null!, ref currentSchema) as JsonSchema;
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
		var idKeyword = Keywords!.OfType<IIdKeyword>().SingleOrDefault();
		return idKeyword?.Id.OriginalString ?? BaseUri.OriginalString;
	}
}

/// <summary>
/// JSON converter for <see cref="JsonSchema"/>.
/// </summary>
public sealed class SchemaJsonConverter : WeaklyTypedJsonConverter<JsonSchema>
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
						var node = options.Read(ref reader, JsonSchemaSerializerContext.Default.JsonNode);
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
						var typeInfo = SchemaKeywordRegistry.GetTypeInfo(keywordType);
						implementation = options.Read(ref reader, keywordType, typeInfo) as IJsonSchemaKeyword ??
					                  throw new InvalidOperationException($"Could not deserialize expected keyword `{keyword}`");
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

			var keywordType = keyword.GetType();
			var typeInfo = SchemaKeywordRegistry.GetTypeInfo(keywordType);
			options.Write(writer, keyword, keywordType, typeInfo);
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