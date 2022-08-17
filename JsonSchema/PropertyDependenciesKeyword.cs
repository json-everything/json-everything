using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema;

[Applicator]
[SchemaKeyword(Name)]
[JsonConverter(typeof(PropertyDependenciesKeywordJsonConverter))]
public class PropertyDependenciesKeyword : IJsonSchemaKeyword, IEquatable<PropertyDependenciesKeyword>
{
	internal const string Name = "propertyDependencies";

	public IReadOnlyDictionary<string, PropertyDependency> Dependencies { get; }

	public PropertyDependenciesKeyword(IReadOnlyDictionary<string, PropertyDependency> dependencies)
	{
		Dependencies = dependencies;
	}

	public void Validate(ValidationContext context)
	{
		throw new NotImplementedException();
	}

	public bool Equals(PropertyDependenciesKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		return Dependencies.Equals(other.Dependencies);
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