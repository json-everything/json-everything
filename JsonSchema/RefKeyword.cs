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
/// Handles `$ref`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Core201909Id)]
[Vocabulary(Vocabularies.Core202012Id)]
[Vocabulary(Vocabularies.CoreNextId)]
[JsonConverter(typeof(RefKeywordJsonConverter))]
public class RefKeyword : IJsonSchemaKeyword, IKeywordHandler
{
	public static RefKeyword Handler { get; } = new(new Uri("https://json-everything.net/"));

	bool IKeywordHandler.Evaluate(FunctionalEvaluationContext context)
	{
		if (!context.LocalSchema.AsObject().TryGetValue(Name, out var requirement, out _)) return true;

		string? reference;
		if (requirement is not JsonValue reqValue || (reference = reqValue.GetString()) is null ||
		    !Uri.TryCreate(reference, UriKind.RelativeOrAbsolute, out _))
			throw new JsonSchemaException("$ref must be a URI string");

		var newUri = new Uri(context.CurrentUri, reference);
		var fragment = newUri.Fragment;

		//var navigation = (newUri.OriginalString, InstanceLocation: instanceLocation);
		//if (context.NavigatedReferences.Contains(navigation))
		//	throw new JsonSchemaException($"Encountered circular reference at schema location `{newUri}` and instance location `{schemaConstraint.RelativeInstanceLocation}`");

		var newBaseUri = new Uri(newUri.GetLeftPart(UriPartial.Query));
		var targetBase = context.SchemaRegistry.GetUntyped(newBaseUri) ??
		                 throw new JsonSchemaException($"Cannot resolve base schema `{newBaseUri}`");
		JsonNode? targetSchema;

		if (string.IsNullOrEmpty(fragment))
			targetSchema = targetBase;
		else if (JsonPointer.TryParse(fragment, out var pointerFragment))
		{
			if (!pointerFragment.TryEvaluate(targetBase, out targetSchema))
				throw new JsonSchemaException($"could not find location {fragment} within base schema {newBaseUri}");
		}
		else
		{
			var anchorFragment = fragment[1..];
			if (!AnchorKeyword.AnchorPattern.IsMatch(anchorFragment))
				throw new JsonSchemaException($"Unrecognized fragment type `{newUri}`");

			throw new NotImplementedException("anchors not yet supported");
		}

		var refContext = context;
		refContext.LocalSchema = targetSchema!;
		refContext.CurrentUri = newBaseUri;

		return JsonSchema.Evaluate(refContext);
	}

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "$ref";

	/// <summary>
	/// The URI reference.
	/// </summary>
	public Uri Reference { get; }

	/// <summary>
	/// Creates a new <see cref="RefKeyword"/>.
	/// </summary>
	/// <param name="value">The URI reference.</param>
	public RefKeyword(Uri value)
	{
		Reference = value;
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
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint, IReadOnlyList<KeywordConstraint> localConstraints, EvaluationContext context)
	{
		var newUri = new Uri(schemaConstraint.SchemaBaseUri, Reference);
		var fragment = newUri.Fragment;

		var instanceLocation = schemaConstraint.BaseInstanceLocation.Combine(schemaConstraint.RelativeInstanceLocation);
		var navigation = (newUri.OriginalString, InstanceLocation: instanceLocation);
		if (context.NavigatedReferences.Contains(navigation))
			throw new JsonSchemaException($"Encountered circular reference at schema location `{newUri}` and instance location `{schemaConstraint.RelativeInstanceLocation}`");

		var newBaseUri = new Uri(newUri.GetLeftPart(UriPartial.Query));

		JsonSchema? targetSchema = null;
		var targetBase = context.Options.SchemaRegistry.Get(newBaseUri) ??
						 throw new JsonSchemaException($"Cannot resolve base schema from `{newUri}`");

		if (JsonPointer.TryParse(fragment, out var pointerFragment))
		{
			if (targetBase == null)
				throw new JsonSchemaException($"Cannot resolve base schema from `{newUri}`");

			targetSchema = targetBase.FindSubschema(pointerFragment!, context.Options);
		}
		else
		{
			var anchorFragment = fragment[1..];
			if (!AnchorKeyword.AnchorPattern.IsMatch(anchorFragment))
				throw new JsonSchemaException($"Unrecognized fragment type `{newUri}`");

			if (targetBase is JsonSchema targetBaseSchema &&
				targetBaseSchema.Anchors.TryGetValue(anchorFragment, out var anchorDefinition))
				targetSchema = anchorDefinition.Schema;
		}

		if (targetSchema == null)
			throw new JsonSchemaException($"Cannot resolve schema `{newUri}`");

		context.NavigatedReferences.Push(navigation);
		var subschemaConstraint = targetSchema.GetConstraint(JsonPointer.Create(Name), schemaConstraint.BaseInstanceLocation, JsonPointer.Empty, context);
		context.NavigatedReferences.Pop();
		if (pointerFragment != null)
			subschemaConstraint.BaseSchemaOffset = pointerFragment;

		return new KeywordConstraint(Name, Evaluator)
		{
			ChildDependencies = [subschemaConstraint]
		};
	}

	private static void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		var subSchemaEvaluation = evaluation.ChildEvaluations.Single();

		if (!subSchemaEvaluation.Results.IsValid)
			evaluation.Results.Fail();
	}
}

/// <summary>
/// JSON converter for <see cref="RefKeyword"/>.
/// </summary>
public sealed class RefKeywordJsonConverter : WeaklyTypedJsonConverter<RefKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="RefKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override RefKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var uri = reader.GetString();
		return new RefKeyword(new Uri(uri!, UriKind.RelativeOrAbsolute));


	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, RefKeyword value, JsonSerializerOptions options)
	{
		options.Write(writer, value.Reference, JsonSchemaSerializerContext.Default.Uri);
	}
}