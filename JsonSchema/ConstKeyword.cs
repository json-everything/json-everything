using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Handles `const`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Validation201909Id)]
[Vocabulary(Vocabularies.Validation202012Id)]
[Vocabulary(Vocabularies.ValidationNextId)]
[JsonConverter(typeof(ConstKeywordJsonConverter))]
public class ConstKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "const";

	/// <summary>
	/// The constant value.
	/// </summary>
	public JsonNode? Value { get; }

	/// <summary>
	/// Creates a new <see cref="ConstKeyword"/>.
	/// </summary>
	/// <param name="value">The constant value.</param>
	public ConstKeyword(JsonNode? value)
	{
		Value = value;
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
		return new KeywordConstraint(Name, Evaluator);
	}

	private void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		if (!evaluation.LocalInstance.IsEquivalentTo(Value))
			evaluation.Results.Fail(Name, ErrorMessages.GetConst(context.Options.Culture), ("value", Value.AsJsonString()));
	}
}

/// <summary>
/// JSON converter for <see cref="ConstKeyword"/>.
/// </summary>
public sealed class ConstKeywordJsonConverter : JsonConverter<ConstKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="ConstKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override ConstKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = JsonSerializer.Deserialize(ref reader, JsonSchemaSerializationContext.Default.JsonNode);

		return new ConstKeyword(node);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, ConstKeyword value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Value, options);
	}
}

public static partial class ErrorMessages
{
	/// <summary>
	/// Gets or sets the error message for <see cref="ConstKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[value]] - the value in the schema
	/// </remarks>
	public static string? Const { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="ConstKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[value]] - the value in the schema
	/// </remarks>
	public static string GetConst(CultureInfo? culture)
	{
		return Const ?? Get(culture);
	}
}