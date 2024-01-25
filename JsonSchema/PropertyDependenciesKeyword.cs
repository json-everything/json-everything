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
public class PropertyDependenciesKeyword : IJsonSchemaKeyword, ICustomSchemaCollector
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

	private static void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		var failedProperties = evaluation.ChildEvaluations
			.Where(x => !x.Results.IsValid)
			.Select(x => x.Results.EvaluationPath.Segments.Last().Value)
			.ToArray();
		evaluation.Results.SetAnnotation(Name, evaluation.ChildEvaluations.Select(x => (JsonNode)x.Results.EvaluationPath.Segments.Last().Value!).ToJsonArray());
		
		if (failedProperties.Length != 0)
			evaluation.Results.Fail(Name, ErrorMessages.GetDependentSchemas(context.Options.Culture), ("failed", failedProperties));
	}

	(JsonSchema?, int) ICustomSchemaCollector.FindSubschema(IReadOnlyList<PointerSegment> segments)
	{
		if (segments.Count < 2) return (null, 0);
		if (!Dependencies.TryGetValue(segments[0].Value, out var property)) return (null, 0);
		if (!property.Schemas.TryGetValue(segments[1].Value, out var schema)) return (null, 0);

		return (schema, 2);
	}
}

/// <summary>
/// JSON converter for <see cref="PropertyDependenciesKeyword"/>.
/// </summary>
public sealed class PropertyDependenciesKeywordJsonConverter : JsonConverter<PropertyDependenciesKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="PropertyDependenciesKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override PropertyDependenciesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var dependencies = JsonSerializer.Deserialize(ref reader, JsonSchemaSerializationContext.Default.DictionaryStringPropertyDependency);

		return new PropertyDependenciesKeyword(dependencies!);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, PropertyDependenciesKeyword value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Dependencies, options);
	}
}