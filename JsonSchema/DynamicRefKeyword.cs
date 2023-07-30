using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `$dynamicRef`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Core202012Id)]
[Vocabulary(Vocabularies.CoreNextId)]
[JsonConverter(typeof(DynamicRefKeywordJsonConverter))]
public class DynamicRefKeyword : IJsonSchemaKeyword, IEquatable<DynamicRefKeyword>
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "$dynamicRef";

	/// <summary>
	/// The URI reference.
	/// </summary>
	public Uri Reference { get; }

	/// <summary>
	/// Creates a new <see cref="DynamicRefKeyword"/>.
	/// </summary>
	/// <param name="value"></param>
	public DynamicRefKeyword(Uri value)
	{
		Reference = value ?? throw new ArgumentNullException(nameof(value));
	}

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint, IReadOnlyList<KeywordConstraint> localConstraints, ConstraintBuilderContext context)
	{
		var newUri = new Uri(context.Scope.LocalScope, Reference);
		var newBaseUri = new Uri(newUri.GetLeftPart(UriPartial.Query));
		var anchorName = Reference.OriginalString.Split('#').Last();

		JsonSchema? targetSchema = null;
		var targetBase = context.Options.SchemaRegistry.Get(newBaseUri) ??
		                 throw new JsonSchemaException($"Cannot resolve base schema from `{newUri}`");

		foreach (var uri in context.Scope.Reverse())
		{
			var scopeRoot = context.Options.SchemaRegistry.Get(uri);
			if (scopeRoot == null)
				throw new Exception("This shouldn't happen");

			if (scopeRoot is not JsonSchema schemaRoot)
				throw new Exception("Does OpenAPI use anchors?");

			if (!schemaRoot.Anchors.TryGetValue(anchorName, out var anchor) || !anchor.IsDynamic) continue;

			if (targetBase is JsonSchema targetBaseSchema &&
			    context.EvaluatingAs == SpecVersion.Draft202012 &&
			    (!targetBaseSchema.Anchors.TryGetValue(anchorName, out var targetAnchor) || !targetAnchor.IsDynamic)) break;

			targetSchema = anchor.Schema;
			break;
		}

		if (targetSchema == null)
		{
			if (JsonPointer.TryParse(newUri.Fragment, out var pointerFragment))
			{
				if (targetBase == null)
					throw new JsonSchemaException($"Cannot resolve base schema from `{newUri}`");

				targetSchema = targetBase.FindSubschema(pointerFragment!, context.Options);
			}
			else
			{
				var anchorFragment = newUri.Fragment.Substring(1);
				if (!AnchorKeyword.AnchorPattern.IsMatch(anchorFragment))
					throw new JsonSchemaException($"Unrecognized fragment type `{newUri}`");

				if (targetBase is JsonSchema targetBaseSchema &&
				    targetBaseSchema.Anchors.TryGetValue(anchorFragment, out var anchorDefinition))
					targetSchema = anchorDefinition.Schema;
			}

			if (targetSchema == null)
				throw new JsonSchemaException($"Cannot resolve schema `{newUri}`");
		}

		return new KeywordConstraint(Name, (e, c) => Evaluator(e, c, targetSchema));
	}

	private static void Evaluator(KeywordEvaluation evaluation, ConstraintBuilderContext context, JsonSchema target)
	{
		var childEvaluation = target
			.GetConstraint(JsonPointer.Create(Name), evaluation.Results.InstanceLocation, JsonPointer.Empty, context)
			.BuildEvaluation(evaluation.LocalInstance, evaluation.Results.InstanceLocation, evaluation.Results.EvaluationPath.Combine(Name), context.Options);
		evaluation.ChildEvaluations = new[] { childEvaluation };

		childEvaluation.Evaluate(context);

		if (!childEvaluation.Results.IsValid)
			evaluation.Results.Fail();
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(DynamicRefKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		return Equals(Reference, other.Reference);
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as DynamicRefKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Reference.GetHashCode();
	}
}

internal class DynamicRefKeywordJsonConverter : JsonConverter<DynamicRefKeyword>
{
	public override DynamicRefKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var uri = reader.GetString()!;
		return new DynamicRefKeyword(new Uri(uri, UriKind.RelativeOrAbsolute));


	}
	public override void Write(Utf8JsonWriter writer, DynamicRefKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(DynamicRefKeyword.Name);
		JsonSerializer.Serialize(writer, value.Reference, options);
	}
}