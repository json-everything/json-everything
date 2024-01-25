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
/// Handles `dependentRequired`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Validation201909Id)]
[Vocabulary(Vocabularies.Validation202012Id)]
[Vocabulary(Vocabularies.ValidationNextId)]
[JsonConverter(typeof(DependentRequiredKeywordJsonConverter))]
public class DependentRequiredKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "dependentRequired";

	/// <summary>
	/// The collection of "required"-type dependencies.
	/// </summary>
	public IReadOnlyDictionary<string, IReadOnlyList<string>> Requirements { get; }

	/// <summary>
	/// Creates a new <see cref="DependentRequiredKeyword"/>.
	/// </summary>
	/// <param name="values">The collection of "required"-type dependencies.</param>
	public DependentRequiredKeyword(IReadOnlyDictionary<string, IReadOnlyList<string>> values)
	{
		Requirements = values ?? throw new ArgumentNullException(nameof(values));
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
		if (evaluation.LocalInstance is not JsonObject obj)
		{
			evaluation.MarkAsSkipped();
			return;
		}

		var existingProperties = obj.Select(x => x.Key).ToArray();

		var missing = new Dictionary<string, string[]>();
		foreach (var requirement in Requirements)
		{
			if (!existingProperties.Contains(requirement.Key)) continue;

			var missingProperties = requirement.Value.Except(existingProperties).ToArray();
			if (missingProperties.Length != 0)
				missing[requirement.Key] = missingProperties;
		}

		if (missing.Count != 0)
			evaluation.Results.Fail(Name, ErrorMessages.GetDependentRequired(context.Options.Culture), ("missing", missing));
	}
}

/// <summary>
/// JSON converter for <see cref="DependentRequiredKeyword"/>.
/// </summary>
public sealed class DependentRequiredKeywordJsonConverter : JsonConverter<DependentRequiredKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="DependentRequiredKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override DependentRequiredKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected object");

		var requirements = JsonSerializer.Deserialize(ref reader, JsonSchemaSerializationContext.Default.DictionaryStringListString)!;
		return new DependentRequiredKeyword(requirements!.ToDictionary(x => x.Key, x => (IReadOnlyList<string>)x.Value));
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, DependentRequiredKeyword value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		foreach (var kvp in value.Requirements)
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
	/// Gets or sets the error message for <see cref="DependentRequiredKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[missing]] - the value in the schema
	/// </remarks>
	public static string? DependentRequired { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="DependentRequiredKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[missing]] - the value in the schema
	/// </remarks>
	public static string GetDependentRequired(CultureInfo? culture)
	{
		return DependentRequired ?? Get(culture);
	}
}