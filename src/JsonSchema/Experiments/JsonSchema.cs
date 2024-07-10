using System;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema.Experiments;

public static class JsonSchema
{
	public static Uri DefaultBaseUri { get; set; } = new("https://json-everything.net/");

	public static EvaluationResults Evaluate(JsonNode schema, JsonNode? instance, EvaluationOptions? options = null)
	{
		options ??= EvaluationOptions.Default;

		if (schema is JsonObject objSchema)
		{
			if (objSchema.ContainsKey("$id"))
				options.SchemaRegistry.Register(objSchema);
			else
			{
				schema = objSchema = (JsonObject)objSchema.DeepClone();
				objSchema["$id"] = options.SchemaRegistry.Register(objSchema).OriginalString;
			}
		}

		var context = new EvaluationContext
		{
			SchemaLocation = JsonPointer.Empty,
			InstanceLocation = JsonPointer.Empty,
			EvaluationPath = JsonPointer.Empty,
			LocalInstance = instance,
			Options = options
		};

		return context.Evaluate(schema);
	}

	private static Uri GenerateId() => new(DefaultBaseUri, Guid.NewGuid().ToString("N")[..10]);
}