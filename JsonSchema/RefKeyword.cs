using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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
public class RefKeyword : IJsonSchemaKeyword
{
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

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint, IReadOnlyList<KeywordConstraint> localConstraints, ConstraintBuilderContext context)
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
			var anchorFragment = fragment.Substring(1);
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
			ChildDependencies = new[] { subschemaConstraint }
		};
	}

	private static void Evaluator(KeywordEvaluation evaluation, ConstraintBuilderContext context)
	{
		var subSchemaEvaluation = evaluation.ChildEvaluations.Single();

		if (!subSchemaEvaluation.Results.IsValid)
			evaluation.Results.Fail();
	}
}

internal class RefKeywordJsonConverter : JsonConverter<RefKeyword>
{
	public override RefKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var uri = reader.GetString();
		return new RefKeyword(new Uri(uri!, UriKind.RelativeOrAbsolute));


	}
	public override void Write(Utf8JsonWriter writer, RefKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(RefKeyword.Name);
		JsonSerializer.Serialize(writer, value.Reference, options);
	}
}