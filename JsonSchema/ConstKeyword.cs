using System;
using System.Collections.Generic;
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

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		EvaluationContext context)
	{
		return new KeywordConstraint(Name, Evaluator);
	}

	private void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		if (!evaluation.LocalInstance.IsEquivalentTo(Value))
			evaluation.Results.Fail(Name, ErrorMessages.Const, ("value", Value.AsJsonString()));
	}
}

internal class ConstKeywordJsonConverter : JsonConverter<ConstKeyword>
{
	public override ConstKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = JsonSerializer.Deserialize<JsonNode>(ref reader, options);

		return new ConstKeyword(node);
	}
	public override void Write(Utf8JsonWriter writer, ConstKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(ConstKeyword.Name);
		JsonSerializer.Serialize(writer, value.Value, options);
	}
}

public static partial class ErrorMessages
{
	private static string? _const;

	/// <summary>
	/// Gets or sets the error message for <see cref="ConstKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[value]] - the value in the schema
	/// </remarks>
	public static string Const
	{
		get => _const ?? Get();
		set => _const = value;
	}
}