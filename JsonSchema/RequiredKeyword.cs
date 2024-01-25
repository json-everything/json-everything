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
/// Handles `requires`.
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
[JsonConverter(typeof(RequiredKeywordJsonConverter))]
public class RequiredKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "required";

	/// <summary>
	/// The required properties.
	/// </summary>
	public IReadOnlyList<string> Properties { get; }

	/// <summary>
	/// Creates a new <see cref="RequiredKeyword"/>.
	/// </summary>
	/// <param name="values">The required properties.</param>
	public RequiredKeyword(params string[] values)
	{
		Properties = values ?? throw new ArgumentNullException(nameof(values));
	}

	/// <summary>
	/// Creates a new <see cref="RequiredKeyword"/>.
	/// </summary>
	/// <param name="values">The required properties.</param>
	public RequiredKeyword(IEnumerable<string> values)
	{
		Properties = values.ToReadOnlyList();
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
		if (evaluation.LocalInstance is not JsonObject obj) return;

		var missing = Properties.Except(obj.Select(x => x.Key)).ToArray();
		if (missing.Length != 0)
			evaluation.Results.Fail(Name, ErrorMessages.GetRequired(context.Options.Culture), ("missing", missing));
	}
}

/// <summary>
/// JSON converter for <see cref="RequiredKeyword"/>.
/// </summary>
public sealed class RequiredKeywordJsonConverter : JsonConverter<RequiredKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="RequiredKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override RequiredKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return new RequiredKeyword(
			JsonSerializer.Deserialize(ref reader, JsonSchemaSerializationContext.Default.StringArray) 
			?? throw new JsonException("Expected array"));
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, RequiredKeyword value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		foreach (var property in value.Properties)
		{
			writer.WriteStringValue(property);
		}
		writer.WriteEndArray();
	}
}

public static partial class ErrorMessages
{
	/// <summary>
	/// Gets or sets the error message for <see cref="RequiredKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[missing]] - the properties missing from the JSON instance
	/// </remarks>
	public static string? Required { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="RequiredKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[missing]] - the properties missing from the JSON instance
	/// </remarks>
	public static string GetRequired(CultureInfo? culture)
	{
		return Required ?? Get(culture);
	}
}