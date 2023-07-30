using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

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

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		ConstraintBuilderContext context)
	{
		return new KeywordConstraint(Name, Evaluator);
	}

	private void Evaluator(KeywordEvaluation evaluation, ConstraintBuilderContext context)
	{
		if (evaluation.LocalInstance is not JsonObject obj) return;

		var missing = Properties.Except(obj.Select(x => x.Key)).ToArray();
		if (missing.Length != 0)
			evaluation.Results.Fail(Name, ErrorMessages.Required, ("missing", missing));
	}
}

internal class RequiredKeywordJsonConverter : JsonConverter<RequiredKeyword>
{
	public override RequiredKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using var document = JsonDocument.ParseValue(ref reader);

		if (document.RootElement.ValueKind != JsonValueKind.Array)
			throw new JsonException("Expected array");

		return new RequiredKeyword(document.RootElement.EnumerateArray()
			.Select(e => e.GetString()!));
	}
	public override void Write(Utf8JsonWriter writer, RequiredKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(RequiredKeyword.Name);
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
	private static string? _required;

	/// <summary>
	/// Gets or sets the error message for <see cref="RequiredKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[missing]] - the properties missing from the JSON instance
	/// </remarks>
	public static string Required
	{
		get => _required ?? Get();
		set => _required = value;
	}
}