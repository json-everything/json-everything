using System;
using System.Linq;
using Json.Pointer;

namespace Json.Schema.DataGeneration.Requirements;

internal class RefRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchema schema, EvaluationOptions options)
	{
		var refKeyword = schema.Keywords?.OfType<RefKeyword>().FirstOrDefault();
		if (refKeyword is null) return;

		var reference = refKeyword.Reference.OriginalString;
		if (!JsonPointer.TryParse(reference, out var pointer))
			throw new NotSupportedException("External references are not supported.");

		var rootSchema = options.SchemaRegistry.Get(schema.BaseUri)!;
		var subschema = rootSchema.FindSubschema(pointer, EvaluationOptions.Default);
		if (subschema is null)
			throw new JsonSchemaException($"Cannot resolve reference `{reference}`");

		var referenceRequirements = subschema.GetRequirements(options);
		context.And(referenceRequirements);
	}
}