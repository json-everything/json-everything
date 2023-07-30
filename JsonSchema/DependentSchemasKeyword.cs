using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `dependentSchemas`.
/// </summary>
[SchemaPriority(10)]
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[Vocabulary(Vocabularies.Applicator202012Id)]
[Vocabulary(Vocabularies.ApplicatorNextId)]
[JsonConverter(typeof(DependentSchemasKeywordJsonConverter))]
public class DependentSchemasKeyword : IJsonSchemaKeyword, IKeyedSchemaCollector
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "dependentSchemas";

	/// <summary>
	/// The collection of "schema"-type dependencies.
	/// </summary>
	public IReadOnlyDictionary<string, JsonSchema> Schemas { get; }

	/// <summary>
	/// Creates a new <see cref="DependentSchemasKeyword"/>.
	/// </summary>
	/// <param name="values">The collection of "schema"-type dependencies.</param>
	public DependentSchemasKeyword(IReadOnlyDictionary<string, JsonSchema> values)
	{
		Schemas = values ?? throw new ArgumentNullException(nameof(values));
	}

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		EvaluationContext context)
	{
		var subschemaConstraints = Schemas.Select(requirement =>
		{
			var subschemaConstraint = requirement.Value.GetConstraint(JsonPointer.Create(Name, requirement.Key), schemaConstraint.BaseInstanceLocation, JsonPointer.Empty, context);
			subschemaConstraint.InstanceLocator = evaluation =>
			{
				if (evaluation.LocalInstance is not JsonObject obj ||
				    !obj.ContainsKey(requirement.Key))
					return Array.Empty<JsonPointer>();

				return JsonPointers.SingleEmptyPointerArray;
			};

			return subschemaConstraint;
		}).ToArray();

		return new KeywordConstraint(Name, Evaluator)
		{
			ChildDependencies = subschemaConstraints
		};
	}

	private static void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		var failedProperties = evaluation.ChildEvaluations
			.Where(x => !x.Results.IsValid)
			.Select(x => x.Results.EvaluationPath.Segments.Last().Value)
			.ToArray();
		evaluation.Results.SetAnnotation(Name, evaluation.ChildEvaluations.Select(x => (JsonNode)x.Results.EvaluationPath.Segments.Last().Value!).ToJsonArray());
		
		if (failedProperties.Length != 0)
			evaluation.Results.Fail(Name, ErrorMessages.DependentSchemas, ("failed", failedProperties));
	}
}

internal class DependentSchemasKeywordJsonConverter : JsonConverter<DependentSchemasKeyword>
{
	public override DependentSchemasKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected object");

		var schema = JsonSerializer.Deserialize<Dictionary<string, JsonSchema>>(ref reader, options)!;
		return new DependentSchemasKeyword(schema);
	}
	public override void Write(Utf8JsonWriter writer, DependentSchemasKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(DependentSchemasKeyword.Name);
		writer.WriteStartObject();
		foreach (var kvp in value.Schemas)
		{
			writer.WritePropertyName(kvp.Key);
			JsonSerializer.Serialize(writer, kvp.Value, options);
		}
		writer.WriteEndObject();
	}
}

public static partial class ErrorMessages
{
	private static string? _dependentSchemas;

	/// <summary>
	/// Gets or sets the error message for <see cref="DependentSchemasKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[value]] - the value in the schema
	/// </remarks>
	public static string DependentSchemas
	{
		get => _dependentSchemas ?? Get();
		set => _dependentSchemas = value;
	}
}