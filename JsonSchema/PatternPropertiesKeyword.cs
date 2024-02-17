using System;
using System.Collections.Concurrent;
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
public class PatternPropertiesKeyword : IJsonSchemaKeyword, IKeyedSchemaCollector, IKeywordHandler
{
	public static PatternPropertiesKeyword Handler { get; } = new(new ConcurrentDictionary<Regex, JsonSchema>());

	bool IKeywordHandler.Evaluate(FunctionalEvaluationContext context)
	{
		if (!context.LocalSchema.AsObject().TryGetValue(Name, out var requirement, out _)) return true;

		if (requirement is not JsonObject patternProperties)
			throw new Exception("patternProperties must be an object");

		if (context.LocalInstance is not JsonObject obj) return true;

		var result = true;
		var evaluated = new HashSet<string>();
		foreach (var pattern in patternProperties)
		{
			var matches = obj.Where(x =>
			{
				try
				{
					return Regex.IsMatch(x.Key, pattern.Key);
				}
				catch
				{
					return false;
				}
			});
			foreach (var match in matches)
			{
				if (!obj.TryGetValue(match.Key, out var instanceProp, out _)) continue;

				var localContext = context;
				localContext.LocalInstance = instanceProp;
				localContext.LocalSchema = pattern.Value!;

				result &= JsonSchema.Evaluate(localContext);
				evaluated.Add(match.Key);
			}
		}

		context.Annotations[Name] = new JsonArray([.. evaluated]);
		return result;
	}

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "patternProperties";

	/// <summary>
	/// The pattern-keyed schemas.
	/// </summary>
	public IReadOnlyDictionary<Regex, JsonSchema> Patterns { get; }
	/// <summary>
	/// If any pattern is invalid or unsupported by <see cref="Regex"/>, it will appear here.
	/// </summary>
	/// <remarks>
	/// All validations will fail if this is populated.
	/// </remarks>
	public IReadOnlyList<string>? InvalidPatterns { get; }

	IReadOnlyDictionary<string, JsonSchema> IKeyedSchemaCollector.Schemas => Patterns.ToDictionary(x => x.Key.ToString(), x => x.Value);

	/// <summary>
	/// Creates a new <see cref="PatternPropertiesKeyword"/>.
	/// </summary>
	/// <param name="values">The pattern-keyed schemas.</param>
	public PatternPropertiesKeyword(IReadOnlyDictionary<Regex, JsonSchema> values)
	{
		Patterns = values ?? throw new ArgumentNullException(nameof(values));
	}
	internal PatternPropertiesKeyword(IReadOnlyDictionary<Regex, JsonSchema> values, IReadOnlyList<string> invalidPatterns)
	{
		Patterns = values ?? throw new ArgumentNullException(nameof(values));
		InvalidPatterns = invalidPatterns;
	}

	/// <summary>
	/// Builds a constraint object for a keyword.
	/// </summary>
	/// <param name="schemaConstraint">The <see cref="SchemaConstraint"/> for the schema object that houses this keyword.</param>
	/// <param name="localConstraints">
	/// The set of other <see cref="KeywordConstraint"/>s that have been processed prior to this one.
	/// Will contain the constraints for keyword dependencies.
	/// </param>
	/// <param name="context">The <see cref="EvaluationContext"/>.</param>
	/// <returns>A constraint object.</returns>
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint, IReadOnlyList<KeywordConstraint> localConstraints, EvaluationContext context)
	{
		var subschemaConstraints = Patterns.Select(pattern =>
		{
			var subschemaConstraint = pattern.Value.GetConstraint(JsonPointer.Create(Name), schemaConstraint.BaseInstanceLocation, JsonPointer.Empty, context);
			subschemaConstraint.InstanceLocator = evaluation =>
			{
				if (evaluation.LocalInstance is not JsonObject obj) return Array.Empty<JsonPointer>();

				var properties = obj.Select(x => x.Key).Where(x => pattern.Key.IsMatch(x));

				return properties.Select(x => JsonPointer.Create(x));
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
		evaluation.Results.SetAnnotation(Name, evaluation.ChildEvaluations.Select(x => (JsonNode)x.RelativeInstanceLocation.Segments[0].Value!).ToJsonArray());
		
		if (!evaluation.ChildEvaluations.All(x => x.Results.IsValid))
			evaluation.Results.Fail();
	}
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
		var schemas = new Dictionary<Regex, JsonSchema>();
		var invalidProps = new List<string>();
		foreach (var prop in patternProps)
		{
			try
			{
				var regex = new Regex(prop.Key, RegexOptions.ECMAScript | RegexOptions.Compiled);
				schemas.Add(regex, prop.Value);
			}
			catch
			{
				invalidProps.Add(prop.Key);
			}
		}
		return new PatternPropertiesKeyword(schemas, invalidProps);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, PatternPropertiesKeyword value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		foreach (var schema in value.Patterns)
		{
			writer.WritePropertyName(schema.Key.ToString());
			options.Write(writer, schema.Value, JsonSchemaSerializerContext.Default.JsonSchema);;
		}
		writer.WriteEndObject();
	}
}