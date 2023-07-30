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
/// Handles the `propertyDependencies` keyword.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.ApplicatorNextId)]
[JsonConverter(typeof(PropertyDependenciesKeywordJsonConverter))]
public class PropertyDependenciesKeyword : IJsonSchemaKeyword, ICustomSchemaCollector, IEquatable<PropertyDependenciesKeyword>
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "propertyDependencies";

	/// <summary>
	/// Gets the collection of dependencies.
	/// </summary>
	public IReadOnlyDictionary<string, PropertyDependency> Dependencies { get; }

	IEnumerable<JsonSchema> ICustomSchemaCollector.Schemas => Dependencies.SelectMany(x => x.Value.Schemas.Select(y => y.Value));
	/// <summary>
	/// Creates a new instance of the <see cref="PropertyDependenciesKeyword"/>.
	/// </summary>
	/// <param name="dependencies">The collection of dependencies.</param>
	public PropertyDependenciesKeyword(IReadOnlyDictionary<string, PropertyDependency> dependencies)
	{
		Dependencies = dependencies;
	}

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		ConstraintBuilderContext context)
	{
		var subschemaConstraints = Dependencies.SelectMany(property =>
		{
			var propertyConstraint = property.Value.Schemas.Select(requirement =>
			{
				var requirementConstraint = requirement.Value.GetConstraint(JsonPointer.Create(Name, property.Key), schemaConstraint.BaseInstanceLocation, JsonPointer.Empty, context);
				requirementConstraint.InstanceLocator = evaluation =>
				{
					if (evaluation.LocalInstance is not JsonObject obj ||
					    !obj.TryGetValue(property.Key, out var value, out _) ||
					    !value.IsEquivalentTo(requirement.Key))
						return Array.Empty<JsonPointer>();

					return JsonPointers.SingleEmptyPointerArray;
				};

				return requirementConstraint;
			});

			return propertyConstraint;
		}).ToArray();

		return new KeywordConstraint(Name, Evaluator)
		{
			ChildDependencies = subschemaConstraints
		};
	}

	private static void Evaluator(KeywordEvaluation evaluation, ConstraintBuilderContext context)
	{
		var failedProperties = evaluation.ChildEvaluations
			.Where(x => !x.Results.IsValid)
			.Select(x => x.Results.EvaluationPath.Segments.Last().Value)
			.ToArray();
		evaluation.Results.SetAnnotation(Name, evaluation.ChildEvaluations.Select(x => (JsonNode)x.Results.EvaluationPath.Segments.Last().Value!).ToJsonArray());
		
		if (failedProperties.Length != 0)
			evaluation.Results.Fail(Name, ErrorMessages.DependentSchemas, ("failed", failedProperties));
	}

	(JsonSchema?, int) ICustomSchemaCollector.FindSubschema(IReadOnlyList<PointerSegment> segments)
	{
		if (segments.Count < 2) return (null, 0);
		if (!Dependencies.TryGetValue(segments[0].Value, out var property)) return (null, 0);
		if (!property.Schemas.TryGetValue(segments[1].Value, out var schema)) return (null, 0);

		return (schema, 2);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(PropertyDependenciesKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		if (Dependencies.Count != other.Dependencies.Count) return false;
		var byKey = Dependencies.Join(other.Dependencies,
				td => td.Key,
				od => od.Key,
				(td, od) => new { ThisDef = td.Value, OtherDef = od.Value })
			.ToArray();
		if (byKey.Length != Dependencies.Count) return false;

		return byKey.All(g => Equals(g.ThisDef, g.OtherDef));
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object? obj)
	{
		return Equals(obj as PropertyDependenciesKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Dependencies.GetStringDictionaryHashCode();
	}
}

internal class PropertyDependenciesKeywordJsonConverter : JsonConverter<PropertyDependenciesKeyword>
{
	public override PropertyDependenciesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var dependencies = JsonSerializer.Deserialize<Dictionary<string, PropertyDependency>>(ref reader, options);

		return new PropertyDependenciesKeyword(dependencies!);
	}

	public override void Write(Utf8JsonWriter writer, PropertyDependenciesKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(PropertyDependenciesKeyword.Name);
		JsonSerializer.Serialize(writer, value.Dependencies, options);
	}
}