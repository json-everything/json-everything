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
/// Handles `items`.
/// </summary>
[Applicator]
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft202012)]
[Vocabulary(Vocabularies.Applicator202012Id)]
[JsonConverter(typeof(PrefixItemsKeywordJsonConverter))]
public class PrefixItemsKeyword : IJsonSchemaKeyword, IRefResolvable, ISchemaCollector, IEquatable<PrefixItemsKeyword>
{
	internal const string Name = "prefixItems";

	/// <summary>
	/// The collection of schemas for the "schema array" form.
	/// </summary>
	public IReadOnlyList<JsonSchema> ArraySchemas { get; }

	IReadOnlyList<JsonSchema> ISchemaCollector.Schemas => ArraySchemas;

	/// <summary>
	/// Creates a new <see cref="PrefixItemsKeyword"/>.
	/// </summary>
	/// <param name="values">The collection of schemas for the "schema array" form.</param>
	/// <remarks>
	/// Using the `params` constructor to build an array-form `items` keyword with a single schema
	/// will confuse the compiler.  To achieve this, you'll need to explicitly specify the array.
	/// </remarks>
	public PrefixItemsKeyword(params JsonSchema[] values)
	{
		ArraySchemas = values.ToList();
	}

	/// <summary>
	/// Creates a new <see cref="PrefixItemsKeyword"/>.
	/// </summary>
	/// <param name="values">The collection of schemas for the "schema array" form.</param>
	public PrefixItemsKeyword(IEnumerable<JsonSchema> values)
	{
		ArraySchemas = values.ToList();
	}

	/// <summary>
	/// Provides validation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the validation process.</param>
	public void Validate(ValidationContext context)
	{
		context.EnterKeyword(Name);
		var schemaValueType = context.LocalInstance.GetSchemaValueType();
		if (schemaValueType != SchemaValueType.Array)
		{
			context.WrongValueKind(schemaValueType);
			return;
		}

		var array = (JsonArray)context.LocalInstance!;
		var overallResult = true;
		var maxEvaluations = Math.Min(ArraySchemas.Count, array.Count);
		for (int i = 0; i < maxEvaluations; i++)
		{
			var schema = ArraySchemas[i];
			var item = array[i];
			context.Push(context.InstanceLocation.Combine(i),
				item ?? JsonNull.SignalNode,
				context.EvaluationPath.Combine(i),
				schema);
			context.Validate();
			overallResult &= context.LocalResult.IsValid;
			context.Pop();
			if (!overallResult && context.ApplyOptimizations) break;
		}

		if (maxEvaluations == array.Count)
			context.LocalResult.SetAnnotation(Name, true);
		else
			context.LocalResult.SetAnnotation(Name, maxEvaluations);

		if (!overallResult)
			context.LocalResult.Fail(Name);
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
	{
		foreach (var schema in ArraySchemas)
		{
			schema.RegisterSubschemas(registry, currentUri);
		}
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(PrefixItemsKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;

		return ArraySchemas.ContentsEqual(other.ArraySchemas);
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as PrefixItemsKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return ArraySchemas.GetUnorderedCollectionHashCode();
	}
}

internal class PrefixItemsKeywordJsonConverter : JsonConverter<PrefixItemsKeyword>
{
	public override PrefixItemsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException("Expected array");

		var schemas = JsonSerializer.Deserialize<List<JsonSchema>>(ref reader, options)!;
		return new PrefixItemsKeyword(schemas);
	}
	public override void Write(Utf8JsonWriter writer, PrefixItemsKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(PrefixItemsKeyword.Name);
		writer.WriteStartArray();
		foreach (var schema in value.ArraySchemas)
		{
			JsonSerializer.Serialize(writer, schema, options);
		}
		writer.WriteEndArray();
	}
}