using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
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

		// ordinarily, loop detection would go here, but I've decided not to support it.  Stackoverflows will ensue.

		var newBaseUri = new Uri(newUri.GetLeftPart(UriPartial.Query));

		JsonSchema? targetSchema = null;
		var targetBase = options.SchemaRegistry.Get(newBaseUri) ??
		                 throw new JsonSchemaException($"Cannot resolve base schema from `{newUri}`");

		if (JsonPointer.TryParse(fragment.AsSpan(), out var pointerFragment))
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

			if (targetBase is JsonSchema targetBaseSchema) 
				targetSchema = FindAnchor(targetBaseSchema, anchorFragment);
		}

		if (targetSchema == null)
			throw new JsonSchemaException($"Cannot resolve schema `{newUri}`");

		var referenceRequirements = targetSchema.GetRequirements(options);
		context.And(referenceRequirements);
	}

	private static JsonSchema? FindAnchor(JsonSchema root, string anchor)
	{
		var queue = new Queue<JsonSchema>();
		queue.Enqueue(root);

		while (queue.Count != 0)
		{
			var subschema = queue.Dequeue();
			if (subschema.BoolValue.HasValue) continue;
			if (subschema.GetAnchor() == anchor) return subschema;

			foreach (var keyword in subschema.Keywords!)
			{
				switch (keyword)
				{
					// ReSharper disable once RedundantAlwaysMatchSubpattern
					case ISchemaContainer { Schema: { } } container:
						queue.Enqueue(container.Schema);
						break;
					case ISchemaCollector collector:
						foreach (var schema in collector.Schemas)
						{
							queue.Enqueue(schema);
						}
						break;
					case IKeyedSchemaCollector collector:
						foreach (var schema in collector.Schemas.Values)
						{
							queue.Enqueue(schema);
						}
						break;
					case ICustomSchemaCollector collector:
						foreach (var schema in collector.Schemas)
						{
							queue.Enqueue(schema);
						}
						break;
				}
			}
		}

		return null;
	}
}