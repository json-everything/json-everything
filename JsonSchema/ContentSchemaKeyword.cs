using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `contentSchema`.
/// </summary>
[SchemaPriority(20)]
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft201909)]
[SchemaDraft(Draft.Draft202012)]
[SchemaDraft(Draft.DraftNext)]
[Vocabulary(Vocabularies.Content201909Id)]
[Vocabulary(Vocabularies.Content202012Id)]
[Vocabulary(Vocabularies.ContentNextId)]
[JsonConverter(typeof(ContentSchemaKeywordJsonConverter))]
public class ContentSchemaKeyword : IJsonSchemaKeyword, IRefResolvable, ISchemaContainer, IEquatable<ContentSchemaKeyword>
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "contentSchema";

	private readonly JsonNode _schemaAsNode;

	/// <summary>
	/// The schema against which to evaluate the content.
	/// </summary>
	public JsonSchema Schema { get; }

	/// <summary>
	/// Creates a new <see cref="ContentSchemaKeyword"/>.
	/// </summary>
	/// <param name="value">The schema against which to evaluate the content.</param>
	public ContentSchemaKeyword(JsonSchema value)
	{
		Schema = value ?? throw new ArgumentNullException(nameof(value));
		_schemaAsNode = JsonSerializer.SerializeToNode(value)!;
	}

	internal ContentSchemaKeyword(JsonSchema value, JsonNode asNode)
	{
		Schema = value ?? throw new ArgumentNullException(nameof(value));
		_schemaAsNode = asNode;
	}

	/// <summary>
	/// Performs evaluation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the evaluation process.</param>
	public void Evaluate(EvaluationContext context)
	{
		context.EnterKeyword(Name);
		var schemaValueType = context.LocalInstance.GetSchemaValueType();
		if (schemaValueType != SchemaValueType.String)
		{
			context.WrongValueKind(schemaValueType);
			return;
		}

		context.Push(context.EvaluationPath.Combine(Name), Schema);
		context.Evaluate();
		context.Pop();
		var result = context.LocalResult.IsValid;
		if (!result)
			context.LocalResult.Fail();
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	public IEnumerable<Requirement> GetRequirements(JsonPointer subschemaPath, DynamicScope scope, JsonPointer instanceLocation, EvaluationOptions options)
	{
		yield return new Requirement(subschemaPath, instanceLocation,
			(_, _, _) => new KeywordResult(Name, subschemaPath, scope.LocalScope, instanceLocation)
			{
				Annotation = _schemaAsNode
			});
	}

	void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
	{
		Schema.RegisterSubschemas(registry, currentUri);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(ContentSchemaKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		return Equals(Schema, other.Schema);
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as ContentSchemaKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Schema.GetHashCode();
	}
}

internal class ContentSchemaKeywordJsonConverter : JsonConverter<ContentSchemaKeyword>
{
	public override ContentSchemaKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var asNode = JsonSerializer.Deserialize<JsonNode>(ref reader, options);
		var schema = asNode.Deserialize<JsonSchema>(options);

		return new ContentSchemaKeyword(schema!, asNode!);
	}
	public override void Write(Utf8JsonWriter writer, ContentSchemaKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(ContentSchemaKeyword.Name);
		JsonSerializer.Serialize(writer, value.Schema, options);
	}
}