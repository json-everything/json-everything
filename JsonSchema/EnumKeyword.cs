using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Handles `enum`.
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
[JsonConverter(typeof(EnumKeywordJsonConverter))]
public class EnumKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "enum";

	private readonly JsonNode?[] _values;

	/// <summary>
	/// Gets or sets whether the keyword will check for unique values when deserializing.
	/// </summary>
	/// <remarks>
	/// The specification states that values SHOULD be unique.  As this is not a "MUST"
	/// requirement, implementations need to support multiple values.  This setting
	/// allows enforcement of unique values.  It is recommended, but off by default
	/// for specification compliance.
	/// </remarks>
	public static bool RequireUniqueValues { get; set; }

	/// <summary>
	/// The collection of enum values.
	/// </summary>
	/// <remarks>
	/// Enum values aren't necessarily strings; they can be of any JSON value.
	/// </remarks>
	public IReadOnlyCollection<JsonNode?> Values => _values;

	/// <summary>
	/// Creates a new <see cref="EnumKeyword"/>.
	/// </summary>
	/// <param name="values">The collection of enum values.</param>
	public EnumKeyword(params JsonNode?[] values)
	{
		_values = values;

		if (RequireUniqueValues && _values.GroupBy(x => x, JsonNodeEqualityComparer.Instance).Any(x => x.Count() > 1))
			throw new ArgumentException("`enum` requires unique values");
	}

	/// <summary>
	/// Creates a new <see cref="EnumKeyword"/>.
	/// </summary>
	/// <param name="values">The collection of enum values.</param>
	public EnumKeyword(IEnumerable<JsonNode?> values)
	{
		_values = values.ToArray();

		if (RequireUniqueValues && _values.GroupBy(x => x, JsonNodeEqualityComparer.Instance).Any(x => x.Count() > 1))
			throw new ArgumentException("`enum` requires unique values");
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
		if (!Values.Contains(evaluation.LocalInstance, JsonNodeEqualityComparer.Instance))
			evaluation.Results.Fail(Name, ErrorMessages.GetEnum(context.Options.Culture), ("received", evaluation.LocalInstance), ("values", Values));
	}
}

/// <summary>
/// JSON converter for <see cref="EnumKeyword"/>.
/// </summary>
public sealed class EnumKeywordJsonConverter : JsonConverter<EnumKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="EnumKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override EnumKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var array = JsonSerializer.Deserialize(ref reader, JsonSchemaSerializationContext.Default.JsonArray)!;
		if (array is null)
			throw new JsonException("Expected an array, but received null");

		return new EnumKeyword((IEnumerable<JsonNode>)array!);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, EnumKeyword value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		foreach (var node in value.Values)
		{
			JsonSerializer.Serialize(writer, node, options);
		}
		writer.WriteEndArray();
	}
}

public static partial class ErrorMessages
{
	/// <summary>
	/// Gets or sets the error message for <see cref="EnumKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[values]] - the available values in the schema
	///
	/// The default messages are static and do not use these tokens as enum values
	/// may be any JSON type and could be quite large.  They are provided to support
	/// custom messages.
	/// </remarks>
	public static string? Enum { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="EnumKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[values]] - the available values in the schema
	///
	/// The default messages are static and do not use these tokens as enum values
	/// may be any JSON type and could be quite large.  They are provided to support
	/// custom messages.
	/// </remarks>
	public static string GetEnum(CultureInfo? culture)
	{
		return Enum ?? Get(culture);
	}
}