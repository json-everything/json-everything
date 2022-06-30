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
/// Handles `contains`.
/// </summary>
[Applicator]
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft6)]
[SchemaDraft(Draft.Draft7)]
[SchemaDraft(Draft.Draft201909)]
[SchemaDraft(Draft.Draft202012)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[Vocabulary(Vocabularies.Applicator202012Id)]
[JsonConverter(typeof(ContainsKeywordJsonConverter))]
public class ContainsKeyword : IJsonSchemaKeyword, IRefResolvable, ISchemaContainer, IEquatable<ContainsKeyword>
{
	internal const string Name = "contains";

	/// <summary>
	/// The schema to match.
	/// </summary>
	public JsonSchema Schema { get; }

	/// <summary>
	/// Creates a new <see cref="ContainsKeyword"/>.
	/// </summary>
	/// <param name="value">The schema to match.</param>
	public ContainsKeyword(JsonSchema value)
	{
		Schema = value ?? throw new ArgumentNullException(nameof(value));
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
			context.LocalResult.Pass();
			context.WrongValueKind(schemaValueType);
			return;
		}

		var array = (JsonArray)context.LocalInstance!;
		var validIndices = new List<int>();
		for (int i = 0; i < array.Count; i++)
		{
			context.Push(context.InstanceLocation.Combine(PointerSegment.Create($"{i}")), array[i] ?? JsonNull.SignalNode);
			Schema.ValidateSubschema(context);
			if (context.LocalResult.IsValid)
				validIndices.Add(i);
			context.Pop();
		}

		var minContainsKeyword = context.LocalSchema.Keywords!.OfType<MinContainsKeyword>().FirstOrDefault();
		if (minContainsKeyword is { Value: 0 })
		{
			context.LocalResult.SetAnnotation(Name, JsonSerializer.SerializeToNode(validIndices));
			context.LocalResult.Pass();
			context.NotApplicable(() => $"{MinContainsKeyword.Name} is 0.");
			return;
		}

		if (validIndices.Any())
		{
			context.LocalResult.SetAnnotation(Name, JsonSerializer.SerializeToNode(validIndices));
			context.LocalResult.Pass();
		}
		else
			context.LocalResult.Fail(Name, ErrorMessages.Contains);
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
	{
		Schema.RegisterSubschemas(registry, currentUri);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(ContainsKeyword? other)
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
		return Equals(obj as ContainsKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Schema.GetHashCode();
	}
}

internal class ContainsKeywordJsonConverter : JsonConverter<ContainsKeyword>
{
	public override ContainsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options)!;

		return new ContainsKeyword(schema);
	}
	public override void Write(Utf8JsonWriter writer, ContainsKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(ContainsKeyword.Name);
		JsonSerializer.Serialize(writer, value.Schema, options);
	}
}

public static partial class ErrorMessages
{
	private static string? _contains;

	/// <summary>
	/// Gets or sets the error message for <see cref="ContainsKeyword"/>.
	/// </summary>
	/// <remarks>No tokens are supported.</remarks>
	public static string Contains
	{
		get => _contains ?? Get();
		set => _contains = value;
	}
}