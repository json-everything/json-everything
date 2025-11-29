using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `type`.
/// </summary>
/// <remarks>
/// This keyword validates the type of the instance.
/// </remarks>
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

	/// <summary>
	/// Gets the singleton instance of the <see cref="TypeKeyword"/>.
	/// </summary>
	public static TypeKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "type";

	/// <summary>
	/// Initializes a new instance of the <see cref="TypeKeyword"/> class.
	/// </summary>
	protected TypeKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
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

	/// <summary>
	/// Builds and registers subschemas based on the specified keyword data within the provided build context.
	/// </summary>
	/// <param name="keyword">The keyword data used to determine which subschemas to build. Cannot be null.</param>
	/// <param name="context">The context in which subschemas are constructed and registered. Cannot be null.</param>
	public void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	/// <summary>
	/// Evaluates the specified keyword using the provided evaluation context and returns the result of the evaluation.
	/// </summary>
	/// <param name="keyword">The keyword data to be evaluated. Cannot be null.</param>
	/// <param name="context">The context in which the keyword evaluation is performed. Cannot be null.</param>
	/// <returns>A KeywordEvaluation object containing the results of the evaluation.</returns>
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
