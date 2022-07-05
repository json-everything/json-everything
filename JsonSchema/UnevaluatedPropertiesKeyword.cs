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
/// Handles `unevaluatedProperties`.
/// </summary>
[Applicator]
[SchemaPriority(30)]
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft201909)]
[SchemaDraft(Draft.Draft202012)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[Vocabulary(Vocabularies.Applicator202012Id)]
[JsonConverter(typeof(UnevaluatedPropertiesKeywordJsonConverter))]
public class UnevaluatedPropertiesKeyword : IJsonSchemaKeyword, IRefResolvable, ISchemaContainer, IEquatable<UnevaluatedPropertiesKeyword>
{
	internal const string Name = "unevaluatedProperties";

	/// <summary>
	/// The schema by which to validation additional properties.
	/// </summary>
	public JsonSchema Schema { get; }

	static UnevaluatedPropertiesKeyword()
	{
		ValidationResults.RegisterConsolidationMethod(ConsolidateAnnotations);
	}
	/// <summary>
	/// Creates a new <see cref="UnevaluatedPropertiesKeyword"/>.
	/// </summary>
	/// <param name="value"></param>
	public UnevaluatedPropertiesKeyword(JsonSchema value)
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
		var scheamValueType = context.LocalInstance.GetSchemaValueType();
		if (scheamValueType != SchemaValueType.Object)
		{
			context.LocalResult.Pass();
			context.WrongValueKind(scheamValueType);
			return;
		}

		context.Options.LogIndentLevel++;
		var overallResult = true;
		List<string> evaluatedProperties;
		var annotation = context.LocalResult.GetAllAnnotations<List<string>>(PropertiesKeyword.Name).SelectMany(x => x).ToList();
		if (!annotation.Any())
		{
			context.Log(() => $"No annotation from {PropertiesKeyword.Name}.");
			evaluatedProperties = new List<string>();
		}
		else
		{
			context.Log(() => $"Annotation from {PropertiesKeyword.Name}: [{string.Join(",", annotation.Select(x => $"'{x}'"))}]");
			evaluatedProperties = annotation;
		}
		annotation = context.LocalResult.GetAllAnnotations<List<string>>(PatternPropertiesKeyword.Name).SelectMany(x => x).ToList();
		if (!annotation.Any())
			context.Log(() => $"No annotation from {PatternPropertiesKeyword.Name}.");
		else
		{
			context.Log(() => $"Annotation from {PatternPropertiesKeyword.Name}: [{string.Join(",", annotation.Select(x => $"'{x}'"))}]");
			evaluatedProperties.AddRange(annotation);
		}
		annotation = context.LocalResult.GetAllAnnotations<List<string>>(AdditionalPropertiesKeyword.Name).SelectMany(x => x).ToList();
		if (!annotation.Any())
			context.Log(() => $"No annotation from {AdditionalPropertiesKeyword.Name}.");
		else
		{
			context.Log(() => $"Annotation from {AdditionalPropertiesKeyword.Name}: [{string.Join(",", annotation.Select(x => $"'{x}'"))}]");
			evaluatedProperties.AddRange(annotation);
		}
		annotation = context.LocalResult.GetAllAnnotations<List<string>>(Name).SelectMany(x => x).ToList();
		if (!annotation.Any())
			context.Log(() => $"No annotation from {Name}.");
		else
		{
			context.Log(() => $"Annotation from {Name}: [{string.Join(",", annotation.Select(x => $"'{x}'"))}]");
			evaluatedProperties.AddRange(annotation);
		}

		var obj = (JsonObject)context.LocalInstance!;
		if (!obj.VerifyJsonObject(context)) return;
	
		var unevaluatedProperties = obj.Where(p => !evaluatedProperties.Contains(p.Key)).ToList();
		evaluatedProperties.Clear();
		foreach (var property in unevaluatedProperties)
		{
			if (!obj.TryGetPropertyValue(property.Key, out var item))
			{
				context.Log(() => $"Property '{property.Key}' does not exist. Skipping.");
				continue;
			}

			context.Log(() => $"Validating property '{property.Key}'.");
			context.Push(context.InstanceLocation.Combine(PointerSegment.Create($"{property.Key}")), item ?? JsonNull.SignalNode);
			Schema.ValidateSubschema(context);
			var localResult = context.LocalResult.IsValid;
			overallResult &= localResult;
			context.Log(() => $"Property '{property.Key}' {localResult.GetValidityString()}.");
			context.Pop();
			if (!overallResult && context.ApplyOptimizations) break;
			if (localResult)
				evaluatedProperties.Add(property.Key);
		}
		context.Options.LogIndentLevel--;

		context.LocalResult.SetAnnotation(Name, evaluatedProperties);
		if (overallResult)
			context.LocalResult.Pass();
		else
			context.LocalResult.Fail();
		context.LocalResult.ConsolidateAnnotations();
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	private static void ConsolidateAnnotations(ValidationResults localResults)
	{
		var allProperties = localResults.NestedResults.Select(c => c.TryGetAnnotation(Name))
			.Where(a => a != null)
			.Cast<List<string>>()
			.SelectMany(a => a)
			.Distinct()
			.ToList();
		if (localResults.TryGetAnnotation(Name) is List<string> annotation)
			annotation.AddRange(allProperties);
		else if (allProperties.Any())
			localResults.SetAnnotation(Name, allProperties);
	}

	void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
	{
		Schema.RegisterSubschemas(registry, currentUri);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(UnevaluatedPropertiesKeyword? other)
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
		return Equals(obj as UnevaluatedPropertiesKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Schema.GetHashCode();
	}
}

internal class UnevaluatedPropertiesKeywordJsonConverter : JsonConverter<UnevaluatedPropertiesKeyword>
{
	public override UnevaluatedPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options)!;

		return new UnevaluatedPropertiesKeyword(schema);
	}
	public override void Write(Utf8JsonWriter writer, UnevaluatedPropertiesKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(UnevaluatedPropertiesKeyword.Name);
		JsonSerializer.Serialize(writer, value.Schema, options);
	}
}