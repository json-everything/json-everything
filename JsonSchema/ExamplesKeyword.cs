using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Handles `examples`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Metadata201909Id)]
[Vocabulary(Vocabularies.Metadata202012Id)]
[Vocabulary(Vocabularies.MetadataNextId)]
[JsonConverter(typeof(ExamplesKeywordJsonConverter))]
public class ExamplesKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "examples";

	/// <summary>
	/// The collection of example values.
	/// </summary>
	public IReadOnlyList<JsonNode?> Values { get; }

	/// <summary>
	/// Creates a new <see cref="ExamplesKeyword"/>.
	/// </summary>
	/// <param name="values">The collection of example values.</param>
	public ExamplesKeyword(params JsonNode?[] values)
	{
		Values = values.ToReadOnlyList() ?? throw new ArgumentNullException(nameof(values));
	}

	/// <summary>
	/// Creates a new <see cref="ExamplesKeyword"/>.
	/// </summary>
	/// <param name="values">The collection of example values.</param>
	public ExamplesKeyword(IEnumerable<JsonNode?> values)
	{
		Values = values.ToReadOnlyList() ?? throw new ArgumentNullException(nameof(values));
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
		return KeywordConstraint.SimpleAnnotation(Name, Values.ToJsonArray());
	}
}

internal class ExamplesKeywordJsonConverter : JsonConverter<ExamplesKeyword>
{
	public override ExamplesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var array = JsonSerializer.Deserialize<JsonArray>(ref reader);
		if (array is null)
			throw new JsonException("Expected an array, but received null");

		return new ExamplesKeyword((IEnumerable<JsonNode>)array!);
	}
	public override void Write(Utf8JsonWriter writer, ExamplesKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(ExamplesKeyword.Name);
		writer.WriteStartArray();
		foreach (var node in value.Values)
		{
			JsonSerializer.Serialize(writer, node, options);
		}
		writer.WriteEndArray();
	}
}