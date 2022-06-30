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
/// Handles `properties`.
/// </summary>
[Applicator]
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft6)]
[SchemaDraft(Draft.Draft7)]
[SchemaDraft(Draft.Draft201909)]
[SchemaDraft(Draft.Draft202012)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[Vocabulary(Vocabularies.Applicator202012Id)]
[JsonConverter(typeof(PropertiesKeywordJsonConverter))]
public class PropertiesKeyword : IJsonSchemaKeyword, IRefResolvable, IKeyedSchemaCollector, IEquatable<PropertiesKeyword>
{
	internal const string Name = "properties";

	/// <summary>
	/// The property schemas.
	/// </summary>
	public IReadOnlyDictionary<string, JsonSchema> Properties { get; }

	IReadOnlyDictionary<string, JsonSchema> IKeyedSchemaCollector.Schemas => Properties;

	/// <summary>
	/// Creates a new <see cref="PropertiesKeyword"/>.
	/// </summary>
	/// <param name="values">The property schemas.</param>
	public PropertiesKeyword(IReadOnlyDictionary<string, JsonSchema> values)
	{
		Properties = values ?? throw new ArgumentNullException(nameof(values));
	}

	/// <summary>
	/// Provides validation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the validation process.</param>
	public void Validate(ValidationContext context)
	{
		context.EnterKeyword(Name);
		var schemaValueType = context.LocalInstance.GetSchemaValueType();
		if (schemaValueType != SchemaValueType.Object)
		{
			context.LocalResult.Pass();
			context.WrongValueKind(schemaValueType);
			return;
		}

		var obj = (JsonObject)context.LocalInstance!;
		if (!obj.VerifyJsonObject(context)) return;

		context.Options.LogIndentLevel++;
		var overallResult = true;
		var evaluatedProperties = new List<string>();
		foreach (var property in Properties)
		{
			context.Log(() => $"Validating property '{property.Key}'.");
			var schema = property.Value;
			var name = property.Key;
			if (!obj.TryGetPropertyValue(name, out var item))
			{
				context.Log(() => $"Property '{property.Key}' does not exist. Skipping.");
				continue;
			}

			context.Push(context.InstanceLocation.Combine(name),
				item ?? JsonNull.SignalNode,
				context.EvaluationPath.Combine(Name, name),
				schema);
			schema.ValidateSubschema(context);
			var localResult = context.LocalResult.IsValid;
			overallResult &= localResult;
			evaluatedProperties.Add(name);
			context.Log(() => $"Property '{property.Key}' {localResult.GetValidityString()}.");
			context.Pop();
			if (!overallResult && context.ApplyOptimizations) break;
		}
		context.Options.LogIndentLevel--;
		context.LocalResult.SetAnnotation(Name, JsonSerializer.SerializeToNode(evaluatedProperties));

		if (overallResult)
			context.LocalResult.Pass();
		else
			context.LocalResult.Fail(Name);
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
	{
		foreach (var schema in Properties.Values)
		{
			schema.RegisterSubschemas(registry, currentUri);
		}
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(PropertiesKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		if (Properties.Count != other.Properties.Count) return false;
		var byKey = Properties.Join(other.Properties,
				td => td.Key,
				od => od.Key,
				(td, od) => new { ThisDef = td.Value, OtherDef = od.Value })
			.ToList();
		if (byKey.Count != Properties.Count) return false;

		return byKey.All(g => Equals(g.ThisDef, g.OtherDef));
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as PropertiesKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Properties.GetStringDictionaryHashCode();
	}
}

internal class PropertiesKeywordJsonConverter : JsonConverter<PropertiesKeyword>
{
	public override PropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected object");

		var schema = JsonSerializer.Deserialize<Dictionary<string, JsonSchema>>(ref reader, options)!;
		return new PropertiesKeyword(schema);
	}
	public override void Write(Utf8JsonWriter writer, PropertiesKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(PropertiesKeyword.Name);
		writer.WriteStartObject();
		foreach (var kvp in value.Properties)
		{
			writer.WritePropertyName(kvp.Key);
			JsonSerializer.Serialize(writer, kvp.Value, options);
		}
		writer.WriteEndObject();
	}
}