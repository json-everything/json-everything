using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `type`.
/// </summary>
public class TypeKeyword : IKeywordHandler
{
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

	public static TypeKeyword Instance { get; } = new();

	public string Name => "type";

	protected TypeKeyword()
	{
	}

	public object? ValidateKeywordValue(JsonElement value)
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
					throw new JsonSchemaException($"A '{Name}' array may only contain strings");

				var type = typeElement.GetString()!;
				if (!_types.TryGetValue(type, out var valueType))
					throw new JsonSchemaException($"'{type}' is not a valid JSON Schema value type");

				finalType |= valueType;
			}

			return finalType;
		}

		throw new JsonSchemaException($"'{Name}' must be either a string or an array of strings");
	}

	public void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		var instanceType = context.Instance.GetSchemaValueType();
		var expectedType = (SchemaValueType)keyword.Value!;
		if (expectedType.HasFlag(instanceType))
			return new KeywordEvaluation
			{
				Keyword = Name,
				IsValid = true
			};

		if (instanceType == SchemaValueType.Integer && expectedType.HasFlag(SchemaValueType.Number))
			return new KeywordEvaluation
			{
				Keyword = Name,
				IsValid = true
			};

		// instance is n.0 and expected type has integer but not number
		if (instanceType == SchemaValueType.Number && expectedType.HasFlag(SchemaValueType.Integer))
		{
			// TODO: consider number handling
			if (context.Instance.TryGetDouble(out var number) && number == Math.Truncate(number))
				return new KeywordEvaluation
				{
					Keyword = Name,
					IsValid = true
				};
		}

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = false,
			Error = ErrorMessages.GetType(context.Options.Culture).
				ReplaceToken("received", instanceType, JsonSchemaSerializerContext.Default.SchemaValueType).
				ReplaceToken("expected", expectedType.ToString().ToLower())
		};
	}
}
