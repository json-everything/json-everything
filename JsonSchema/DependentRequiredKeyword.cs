using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `dependentRequired`.
/// </summary>
[SchemaPriority(10)]
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
			evaluation.Results.Fail(Name, ErrorMessages.DependentRequired, ("missing", missing));
	}
}

internal class DependentRequiredKeywordJsonConverter : JsonConverter<DependentRequiredKeyword>
{
	public override DependentRequiredKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected object");

		var requirements = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(ref reader, options);
		return new DependentRequiredKeyword(requirements!.ToDictionary(x => x.Key, x => (IReadOnlyList<string>)x.Value));
	}
	public override void Write(Utf8JsonWriter writer, DependentRequiredKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(DependentRequiredKeyword.Name);
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
	private static string? _dependentRequired;

	/// <summary>
	/// Gets or sets the error message for <see cref="DependentRequiredKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[missing]] - the value in the schema
	/// </remarks>
	public static string DependentRequired
	{
		get => _dependentRequired ?? Get();
		set => _dependentRequired = value;
	}
}