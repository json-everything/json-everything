﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `dependencies`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[JsonConverter(typeof(DependenciesKeywordJsonConverter))]
public class DependenciesKeyword : IJsonSchemaKeyword, IKeyedSchemaCollector
{
	private readonly IReadOnlyDictionary<string, JsonSchema> _schemas;

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "dependencies";

	/// <summary>
	/// The collection of dependencies.
	/// </summary>
	public IReadOnlyDictionary<string, SchemaOrPropertyList> Requirements { get; }

	IReadOnlyDictionary<string, JsonSchema> IKeyedSchemaCollector.Schemas => _schemas;

	/// <summary>
	/// Creates a new <see cref="DependenciesKeyword"/>.
	/// </summary>
	/// <param name="values">The collection of dependencies.</param>
	public DependenciesKeyword(IReadOnlyDictionary<string, SchemaOrPropertyList> values)
	{
		Requirements = values;
		_schemas = Requirements.Where(x => x.Value.Schema != null).ToDictionary(x => x.Key, x => x.Value.Schema!);
	}

	/// <summary>
	/// Builds a constraint object for a keyword.
	/// </summary>
	/// <param name="schemaConstraint">The <see cref="SchemaConstraint"/> for the schema object that houses this keyword.</param>
	/// <param name="localConstraints">
	///     The set of other <see cref="KeywordConstraint"/>s that have been processed prior to this one.
	///     Will contain the constraints for keyword dependencies.
	/// </param>
	/// <param name="context">The <see cref="EvaluationContext"/>.</param>
	/// <returns>A constraint object.</returns>
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		ReadOnlySpan<KeywordConstraint> localConstraints,
		EvaluationContext context)
	{
		var subschemaConstraints = Requirements
			.Where(x => x.Value.Schema != null)
			.Select(requirement =>
		{
			var subschemaConstraint = requirement.Value.Schema!.GetConstraint(JsonPointer.Create(Name, requirement.Key), schemaConstraint.BaseInstanceLocation, JsonPointer.Empty, context);
			subschemaConstraint.InstanceLocator = evaluation =>
			{
				if (evaluation.LocalInstance is not JsonObject obj ||
				    !obj.ContainsKey(requirement.Key))
					return Array.Empty<JsonPointer>();

				return CommonJsonPointers.SingleEmptyPointerArray;
			};

			return subschemaConstraint;
		}).ToArray();

		return new KeywordConstraint(Name, Evaluator)
		{
			ChildDependencies = subschemaConstraints
		};
	}

	private void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		if (evaluation.LocalInstance is not JsonObject obj)
		{
			evaluation.MarkAsSkipped();
			return;
		}

		var existingProperties = obj.Select(x => x.Key).ToArray();

		var missing = new Dictionary<string, string[]>();
		foreach (var requirement in Requirements.Where(x => x.Value.Requirements != null))
		{
			if (!existingProperties.Contains(requirement.Key)) continue;

			var missingProperties = requirement.Value.Requirements!.Except(existingProperties).ToArray();
			if (missingProperties.Length != 0)
				missing[requirement.Key] = missingProperties;
		}

		if (missing.Count != 0)
			evaluation.Results.Fail(Name, ErrorMessages.GetDependentRequired(context.Options.Culture)
				.ReplaceToken("missing", missing));


		var failedProperties = evaluation.ChildEvaluations
			.Where(x => !x.Results.IsValid);

		// ReSharper disable PossibleMultipleEnumeration
		if (!failedProperties.Any()) return;
		
		var properties = failedProperties.Select(x => x.Results.EvaluationPath[^1]).ToArray();
		evaluation.Results.Fail(Name, ErrorMessages.GetDependentSchemas(context.Options.Culture)
				.ReplaceToken("failed", properties));
		// ReSharper restore PossibleMultipleEnumeration
	}
}

/// <summary>
/// JSON converter for <see cref="DependenciesKeyword"/>.
/// </summary>
public sealed class DependenciesKeywordJsonConverter : WeaklyTypedJsonConverter<DependenciesKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="DependenciesKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override DependenciesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected object");

		var dependencies = options.ReadDictionary(ref reader, JsonSchemaSerializerContext.Default.SchemaOrPropertyList)!;
		return new DependenciesKeyword(dependencies);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, DependenciesKeyword value, JsonSerializerOptions options)
	{
		options.WriteDictionary(writer, value.Requirements, JsonSchemaSerializerContext.Default.SchemaOrPropertyList);
	}
}

/// <summary>
/// A holder for either a schema dependency or a requirements dependency.
/// </summary>
[JsonConverter(typeof(SchemaOrPropertyListJsonConverter))]
public class SchemaOrPropertyList
{
	/// <summary>
	/// The schema dependency.
	/// </summary>
	public JsonSchema? Schema { get; }
	/// <summary>
	/// The property dependency.
	/// </summary>
	public List<string>? Requirements { get; }

	/// <summary>
	/// Creates a schema dependency.
	/// </summary>
	/// <param name="schema">The schema dependency.</param>
	public SchemaOrPropertyList(JsonSchema schema)
	{
		Schema = schema;
	}

	/// <summary>
	/// Creates a property dependency.
	/// </summary>
	/// <param name="requirements">The property dependency.</param>
	public SchemaOrPropertyList(IEnumerable<string> requirements)
	{
		Requirements = requirements.ToList();
	}

	/// <summary>
	/// Implicitly creates a <see cref="SchemaOrPropertyList"/> from a <see cref="JsonSchema"/>.
	/// </summary>
	public static implicit operator SchemaOrPropertyList(JsonSchema schema)
	{
		return new SchemaOrPropertyList(schema);
	}

	/// <summary>
	/// Implicitly creates a <see cref="SchemaOrPropertyList"/> from a list of strings.
	/// </summary>
	public static implicit operator SchemaOrPropertyList(List<string> requirements)
	{
		return new SchemaOrPropertyList(requirements);
	}

	/// <summary>
	/// Implicitly creates a <see cref="SchemaOrPropertyList"/> from an array of strings.
	/// </summary>
	public static implicit operator SchemaOrPropertyList(string[] requirements)
	{
		return new SchemaOrPropertyList(requirements);
	}
}

/// <summary>
/// JSON converter for <see cref="SchemaOrPropertyList"/>.
/// </summary>
public sealed class SchemaOrPropertyListJsonConverter : WeaklyTypedJsonConverter<SchemaOrPropertyList>
{
	/// <summary>Reads and converts the JSON to type <see cref="SchemaOrPropertyList"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override SchemaOrPropertyList Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.StartArray)
			return new SchemaOrPropertyList(options.ReadList(ref reader, JsonSchemaSerializerContext.Default.String)!);

		return new SchemaOrPropertyList(options.Read(ref reader, JsonSchemaSerializerContext.Default.JsonSchema)!);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, SchemaOrPropertyList value, JsonSerializerOptions options)
	{
		if (value.Schema != null)
			options.Write(writer, value.Schema, JsonSchemaSerializerContext.Default.JsonSchema);
		else
			options.WriteList(writer, value.Requirements!, JsonSchemaSerializerContext.Default.String);
	}
}
