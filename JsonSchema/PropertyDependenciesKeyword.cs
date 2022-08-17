using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

[Applicator]
[SchemaKeyword(Name)]
[JsonConverter(typeof(PropertyDependenciesKeywordJsonConverter))]
public class PropertyDependenciesKeyword : IJsonSchemaKeyword, IRefResolvable, IEquatable<PropertyDependenciesKeyword>
{
	internal const string Name = "propertyDependencies";

	public IReadOnlyDictionary<string, PropertyDependency> Dependencies { get; }

	static PropertyDependenciesKeyword()
	{
		ValidationResults.RegisterConsolidationMethod(ConsolidateAnnotations);
	}

	public PropertyDependenciesKeyword(IReadOnlyDictionary<string, PropertyDependency> dependencies)
	{
		Dependencies = dependencies;
	}

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
		foreach (var property in Dependencies)
		{
			context.Log(() => $"Validating property '{property.Key}'.");
			var dependency = property.Value;
			var name = property.Key;
			if (!obj.TryGetPropertyValue(name, out var value))
			{
				context.Log(() => $"Property '{property.Key}' does not exist. Skipping.");
				continue;
			}

			if (value.GetSchemaValueType() != SchemaValueType.String)
			{
				context.Log(() => $"Property '{property.Key}' is not a string. Skipping.");
				continue;
			}

			var stringValue = value!.GetValue<string>();
			if (!dependency.Schemas.TryGetValue(stringValue, out var schema))
			{
				context.Log(() => $"Property '{property.Key}' does not specify a requirement for value '{stringValue}'");
				continue;
			}

			context.Push(subschemaLocation: context.SchemaLocation.Combine(PointerSegment.Create(name), PointerSegment.Create(stringValue)));
			schema.ValidateSubschema(context);
			var localResult = context.LocalResult.IsValid;
			overallResult &= localResult;
			context.Log(() => $"Property '{property.Key}' {localResult.GetValidityString()}.");
			context.Pop();
			if (!overallResult && context.ApplyOptimizations) break;
		}
		context.Options.LogIndentLevel--;

		if (overallResult)
			context.LocalResult.Pass();
		else
			context.LocalResult.Fail();
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
		foreach (var schema in Dependencies.Values.SelectMany(x => x.Schemas.Values))
		{
			schema.RegisterSubschemas(registry, currentUri);
		}
	}

	public bool Equals(PropertyDependenciesKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		if (Dependencies.Count != other.Dependencies.Count) return false;
		var byKey = Dependencies.Join(other.Dependencies,
				td => td.Key,
				od => od.Key,
				(td, od) => new { ThisDef = td.Value, OtherDef = od.Value })
			.ToList();
		if (byKey.Count != Dependencies.Count) return false;

		return byKey.All(g => Equals(g.ThisDef, g.OtherDef));
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as PropertyDependenciesKeyword);
	}

	public override int GetHashCode()
	{
		return Dependencies.GetStringDictionaryHashCode();
	}
}

internal class PropertyDependenciesKeywordJsonConverter : JsonConverter<PropertyDependenciesKeyword>
{
	public override PropertyDependenciesKeyword? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, PropertyDependenciesKeyword value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}