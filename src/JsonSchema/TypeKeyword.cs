using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace Json.Schema;

/// <summary>
/// Handles `type`.
/// </summary>
//[SchemaKeyword(Name)]
//[SchemaSpecVersion(SpecVersion.Draft6)]
//[SchemaSpecVersion(SpecVersion.Draft7)]
//[SchemaSpecVersion(SpecVersion.Draft201909)]
//[SchemaSpecVersion(SpecVersion.Draft202012)]
//[SchemaSpecVersion(SpecVersion.DraftNext)]
//[Vocabulary(Vocabularies.Validation201909Id)]
//[Vocabulary(Vocabularies.Validation202012Id)]
//[Vocabulary(Vocabularies.ValidationNextId)]
public class TypeKeyword : IKeywordHandler
{
	public string Name => "type";

	private static readonly ImmutableDictionary<string, SchemaValueType> _types =
		new Dictionary<string, SchemaValueType>
		{
			{ "array", SchemaValueType.Array },
			{ "object", SchemaValueType.Object },
			{ "string", SchemaValueType.String },
			{ "number", SchemaValueType.Number },
			{ "integer", SchemaValueType.Integer },
			{ "boolean", SchemaValueType.Boolean },
			{ "null", SchemaValueType.Null }
		}.ToImmutableDictionary();

	public object? ValidateValue(JsonElement value)
	{
		if (value.ValueKind == JsonValueKind.String)
		{
			var typeName = value.GetString();
			if (!_types.TryGetValue(typeName!, out var valueType))
				throw new JsonSchemaException($"'{typeName}' is not a valid JSON Schema value type");

			return valueType;
		}

		if (value.ValueKind == JsonValueKind.Array)
		{
			SchemaValueType finalType = 0;
			foreach (var typeElement in value.EnumerateArray())
			{
				if (typeElement.ValueKind != JsonValueKind.String)
					throw new JsonSchemaException("A type array may only contain strings");

				var type = typeElement.GetString();
				if (!_types.TryGetValue(type, out var valueType))
					throw new JsonSchemaException($"'{type}' is not a valid JSON Schema value type");

				finalType |= valueType;
			}

			return finalType;
		}

		throw new JsonSchemaException("'type' must be either a string or an array of strings");
	}

	public JsonSchemaNode[] BuildSubschemas(BuildContext context) => [];

	public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		throw new NotImplementedException();

		//var instanceType = context.Instance.GetSchemaValueType();
		//var expectedType = (SchemaValueType) keyword.Value!;
		//if (expectedType.HasFlag(instanceType))
		//	return new KeywordEvaluation
		//	{
		//		Keyword = Name,
		//		IsValid = true
		//	};

		//if (instanceType == SchemaValueType.Integer && expectedType.HasFlag(SchemaValueType.Number))
		//	return new KeywordEvaluation
		//	{
		//		Keyword = Name,
		//		IsValid = true
		//	};

		//if (instanceType == SchemaValueType.Number)
		//{
		//	var number = context.Instance.GetNumber(context.Options.NumberProcessing); // TODO: hm...  Can't do a GetNumber now
		//	if (number == Math.Truncate(number!.Value) && expectedType.HasFlag(SchemaValueType.Integer)) return;
		//}

		//var expected = expectedType.ToString().ToLower();
		//evaluation.Results.Fail(Name, ErrorMessages.GetType(context.Options.Culture).
		//	ReplaceToken("received", instanceType, JsonSchemaSerializerContext.Default.SchemaValueType).
		//	ReplaceToken("expected", expected));
	}
}

public static partial class ErrorMessages
{
	/// <summary>
	/// Gets or sets the error message for <see cref="TypeKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the type of value provided in the JSON instance
	///   - [[expected]] - the type(s) required by the schema
	/// </remarks>
	public static string? Type { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="TypeKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the type of value provided in the JSON instance
	///   - [[expected]] - the type(s) required by the schema
	/// </remarks>
	public static string GetType(CultureInfo? culture)
	{
		return Type ?? Get(culture);
	}
}