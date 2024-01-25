using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `dependentSchemas`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[Vocabulary(Vocabularies.Applicator202012Id)]
[Vocabulary(Vocabularies.ApplicatorNextId)]
[JsonConverter(typeof(DependentSchemasKeywordJsonConverter))]
public class DependentSchemasKeyword : IJsonSchemaKeyword, IKeyedSchemaCollector
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "dependentSchemas";

	/// <summary>
	/// The collection of "schema"-type dependencies.
	/// </summary>
	public IReadOnlyDictionary<string, JsonSchema> Schemas { get; }

	/// <summary>
	/// Creates a new <see cref="DependentSchemasKeyword"/>.
	/// </summary>
	/// <param name="values">The collection of "schema"-type dependencies.</param>
	public DependentSchemasKeyword(IReadOnlyDictionary<string, JsonSchema> values)
	{
		Schemas = values ?? throw new ArgumentNullException(nameof(values));
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
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		EvaluationContext context)
	{
		var subschemaConstraints = Schemas.Select(requirement =>
		{
			var subschemaConstraint = requirement.Value.GetConstraint(JsonPointer.Create(Name, requirement.Key), schemaConstraint.BaseInstanceLocation, JsonPointer.Empty, context);
			subschemaConstraint.InstanceLocator = evaluation =>
			{
				if (evaluation.LocalInstance is not JsonObject obj ||
				    !obj.ContainsKey(requirement.Key))
					return Array.Empty<JsonPointer>();

				return JsonPointers.SingleEmptyPointerArray;
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
		var failedProperties = evaluation.ChildEvaluations
			.Where(x => !x.Results.IsValid)
			.Select(x => x.Results.EvaluationPath.Segments.Last().Value)
			.ToArray();
		evaluation.Results.SetAnnotation(Name, evaluation.ChildEvaluations.Select(x => (JsonNode)x.Results.EvaluationPath.Segments.Last().Value!).ToJsonArray());
		
		if (failedProperties.Length != 0)
			evaluation.Results.Fail(Name, ErrorMessages.GetDependentSchemas(context.Options.Culture), ("failed", failedProperties));
	}
}

/// <summary>
/// JSON converter for <see cref="DependentSchemasKeyword"/>.
/// </summary>
public sealed class DependentSchemasKeywordJsonConverter : JsonConverter<DependentSchemasKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="DependentSchemasKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override DependentSchemasKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected object");

		var schema = JsonSerializer.Deserialize(ref reader, JsonSchemaSerializationContext.Default.DictionaryStringJsonSchema)!;
		return new DependentSchemasKeyword(schema);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, DependentSchemasKeyword value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		foreach (var kvp in value.Schemas)
		{
			writer.WritePropertyName(kvp.Key);
			JsonSerializer.Serialize(writer, kvp.Value, options);
		}
		writer.WriteEndObject();
	}
}

public static partial class ErrorMessages
{
	/// <summary>
	/// Gets or sets the error message for <see cref="DependentSchemasKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[value]] - the value in the schema
	/// </remarks>
	public static string? DependentSchemas { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="DependentSchemasKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[value]] - the value in the schema
	/// </remarks>
	public static string GetDependentSchemas(CultureInfo? culture)
	{
		return DependentSchemas ?? Get(culture);
	}
}