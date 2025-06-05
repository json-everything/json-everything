using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `patternProperties`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[Vocabulary(Vocabularies.Applicator202012Id)]
[Vocabulary(Vocabularies.ApplicatorNextId)]
[JsonConverter(typeof(PatternPropertiesKeywordJsonConverter))]
public class PatternPropertiesKeyword : IJsonSchemaKeyword, IKeyedSchemaCollector
{
	private readonly IReadOnlyDictionary<string, (RegexOrPattern Regex, JsonPointer_Old Pointer, JsonSchema Schema)> _patternsLookup;

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "patternProperties";

	private Dictionary<string, JsonSchema>? _patternValues;
	private Dictionary<Regex, JsonSchema>? _patterns;

	/// <summary>
	/// The pattern values of this PatternPropertiesKeyword
	/// </summary>
	public IReadOnlyDictionary<string, JsonSchema> PatternValues => _patternValues ??= _patternsLookup.ToDictionary(x => x.Key, x => x.Value.Schema);
	
	/// <summary>
	/// The regex patterns of this PatternPropertiesKeyword
	/// </summary>
	[Obsolete($"Please use the '{nameof(PatternValues)}' instead.")]
	public IReadOnlyDictionary<Regex, JsonSchema> Patterns => _patterns ??= _patternsLookup.ToDictionary(x => x.Value.Regex.ToRegex(), x => x.Value.Schema);

	/// <remarks>
	/// (obsolete) All validations will fail if this is populated.
	/// </remarks>
	[Obsolete("This property is not used and will be removed with the next major version.")]
	public IReadOnlyList<string>? InvalidPatterns { get; }

	IReadOnlyDictionary<string, JsonSchema> IKeyedSchemaCollector.Schemas => PatternValues;

	/// <summary>
	/// Creates a new <see cref="PatternPropertiesKeyword"/>.
	/// </summary>
	/// <param name="values">The pattern-keyed schemas.</param>
	public PatternPropertiesKeyword(IEnumerable<KeyValuePair<string, JsonSchema>> values)
	{
		_patternsLookup = values.ToDictionary(x => x.Key, x => (new RegexOrPattern(x.Key), JsonPointer_Old.Create(Name, x.Key), x.Value), StringComparer.Ordinal);
	}

	/// <summary>
	/// Creates a new <see cref="PatternPropertiesKeyword"/>.
	/// </summary>
	/// <param name="values">The pattern-keyed schemas.</param>
	public PatternPropertiesKeyword(IEnumerable<KeyValuePair<Regex, JsonSchema>> values)
	{
		_patternsLookup = values.ToDictionary(x => x.Key.ToString(), x => (new RegexOrPattern(x.Key), JsonPointer_Old.Create(Name, x.Key.ToString()), x.Value), StringComparer.Ordinal);
	}

	internal PatternPropertiesKeyword(IEnumerable<(string Pattern, JsonSchema Schema)> values)
	{
		_patternsLookup = values.ToDictionary(x => x.Pattern, x => (new RegexOrPattern(x.Pattern), JsonPointer_Old.Create(Name, x.Pattern), x.Schema), StringComparer.Ordinal);
	}

	internal PatternPropertiesKeyword(IEnumerable<(Regex Pattern, JsonSchema Schema)> values)
	{
		_patternsLookup = values.ToDictionary(x => x.Pattern.ToString(), x => (new RegexOrPattern(x.Pattern), JsonPointer_Old.Create(Name, x.Pattern.ToString()), x.Schema), StringComparer.Ordinal);
	}

	/// <summary>
	/// Builds a constraint object for a keyword.
	/// </summary>
	/// <param name="schemaConstraint">The <see cref="SchemaConstraint"/> for the schema object that houses this keyword.</param>
	/// <param name="localConstraints">
	///     The set of other <see cref="KeywordConstraint"/>s that have been processed prior to this one.
	///     Will contain the constraints for keyword dependencies.
	/// </param>
	/// <param name="context">The <see cref="EvaluationContext"/>.</param>
	/// <returns>A constraint object.</returns>
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint, ReadOnlySpan<KeywordConstraint> localConstraints, EvaluationContext context)
	{
		var subschemaConstraints = _patternsLookup.Select(kvp =>
		{
			var key = kvp.Key;
			var (regex, pointer, schema) = kvp.Value;

			context.PushEvaluationPath(key);
			var subschemaConstraint = schema.GetConstraint(pointer, schemaConstraint.BaseInstanceLocation, JsonPointer_Old.Empty, context);
			context.PopEvaluationPath();
			subschemaConstraint.InstanceLocator = evaluation =>
			{
				if (evaluation.LocalInstance is not JsonObject obj) return [];

				var properties = obj.Select(x => x.Key).Where(x => regex.IsMatch(x));

				return properties.Select(x => JsonPointer_Old.Create(x));
			};

			return subschemaConstraint;
		}).ToArray();

		return new KeywordConstraint(Name, Evaluator)
		{
			ChildDependencies = subschemaConstraints
		};
	}

	private static void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		evaluation.Results.SetAnnotation(Name, evaluation.ChildEvaluations.Select(x => (JsonNode)x.RelativeInstanceLocation[0]).ToJsonArray());

		if (!evaluation.ChildEvaluations.All(x => x.Results.IsValid))
			evaluation.Results.Fail();
	}

	internal IEnumerable<(string Pattern, JsonSchema Schema)> EnumeratePropertyPatterns() => _patternsLookup.Select(x => (x.Key, x.Value.Schema));
}

/// <summary>
/// JSON converter for <see cref="PatternPropertiesKeyword"/>.
/// </summary>
public sealed class PatternPropertiesKeywordJsonConverter : WeaklyTypedJsonConverter<PatternPropertiesKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="PatternPropertiesKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override PatternPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected object");

		var patternProps = options.ReadDictionary(ref reader, JsonSchemaSerializerContext.Default.JsonSchema)!;
		return new PatternPropertiesKeyword(patternProps);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, PatternPropertiesKeyword value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		foreach (var (pattern, schema) in value.EnumeratePropertyPatterns())
		{
			writer.WritePropertyName(pattern);
			options.Write(writer, schema, JsonSchemaSerializerContext.Default.JsonSchema);
		}
		writer.WriteEndObject();
	}
}