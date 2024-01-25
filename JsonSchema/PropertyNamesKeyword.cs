using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `propertyNames`.
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
[JsonConverter(typeof(PropertyNamesKeywordJsonConverter))]
public class PropertyNamesKeyword : IJsonSchemaKeyword, ISchemaContainer
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "propertyNames";

	/// <summary>
	/// The schema to match.
	/// </summary>
	public JsonSchema Schema { get; }

	/// <summary>
	/// Creates a new <see cref="PropertyNamesKeyword"/>.
	/// </summary>
	/// <param name="value">The schema to match.</param>
	public PropertyNamesKeyword(JsonSchema value)
	{
		Schema = value ?? throw new ArgumentNullException(nameof(value));
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
		var subschemaConstraint = Schema.GetConstraint(JsonPointer.Create(Name), schemaConstraint.BaseInstanceLocation, JsonPointer.Empty, context);
		subschemaConstraint.InstanceLocator = evaluation =>
		{
			if (evaluation.LocalInstance is not JsonObject obj) return Array.Empty<JsonPointer>();

			var properties = obj.Select(x => x.Key);

			return properties.Select(x => JsonPointer.Create(x));
		};
		subschemaConstraint.UseLocatorAsInstance = true;

		return new KeywordConstraint(Name, Evaluator)
		{
			ChildDependencies = new[] { subschemaConstraint }
		};
	}

	private static void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		if (!evaluation.ChildEvaluations.All(x => x.Results.IsValid))
			evaluation.Results.Fail();
	}
}

/// <summary>
/// JSON converter for <see cref="PropertyNamesKeyword"/>.
/// </summary>
public sealed class PropertyNamesKeywordJsonConverter : JsonConverter<PropertyNamesKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="PropertyNamesKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override PropertyNamesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var schema = JsonSerializer.Deserialize(ref reader, JsonSchemaSerializationContext.Default.JsonSchema)!;

		return new PropertyNamesKeyword(schema);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, PropertyNamesKeyword value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Schema, options);
	}
}