using System;
using System.Linq;
using System.Text.RegularExpressions;
using Json.Pointer;

namespace Json.Schema.DataGeneration.Requirements;

internal class RefRequirementsGatherer : IRequirementsGatherer
{
	private static readonly Regex _anchorPattern = new("^[A-Za-z][-A-Za-z0-9.:_]*$");

	public void AddRequirements(RequirementsContext context, JsonSchema schema, EvaluationOptions options)
	{
		var refKeyword = schema.Keywords?.OfType<RefKeyword>().FirstOrDefault();
		if (refKeyword is null) return;

		var newUri = new Uri(schema.BaseUri, refKeyword.Reference);
		var fragment = newUri.Fragment;

		//var instanceLocation = schemaConstraint.BaseInstanceLocation.Combine(schemaConstraint.RelativeInstanceLocation);
		//var navigation = (newUri.OriginalString, InstanceLocation: instanceLocation);
		//if (context.NavigatedReferences.Contains(navigation))
		//	throw new JsonSchemaException($"Encountered circular reference at schema location `{newUri}` and instance location `{schemaConstraint.RelativeInstanceLocation}`");

		var newBaseUri = new Uri(newUri.GetLeftPart(UriPartial.Query));

		JsonSchema? targetSchema = null;
		var targetBase = options.SchemaRegistry.Get(newBaseUri) ??
		                 throw new JsonSchemaException($"Cannot resolve base schema from `{newUri}`");

		if (JsonPointer.TryParse(fragment, out var pointerFragment))
		{
			if (targetBase == null)
				throw new JsonSchemaException($"Cannot resolve base schema from `{newUri}`");

			targetSchema = targetBase.FindSubschema(pointerFragment, options);
		}
		else
		{
			var anchorFragment = fragment[1..];
			if (!_anchorPattern.IsMatch(anchorFragment))
				throw new JsonSchemaException($"Unrecognized fragment type `{newUri}`");

			//if (targetBase is JsonSchema targetBaseSchema &&
			//    targetBaseSchema.Anchors.TryGetValue(anchorFragment, out var anchorDefinition))
			//	targetSchema = anchorDefinition.Schema;
		}

		if (targetSchema == null)
			throw new JsonSchemaException($"Cannot resolve schema `{newUri}`");

		var referenceRequirements = targetSchema.GetRequirements(options);
		context.And(referenceRequirements);
	}
}