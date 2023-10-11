using System.Collections.Generic;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Linting;

public static class JsonSchemaAnalyzer
{
	public static IEnumerable<Diagnostic> Analyze(this JsonSchema schema)
	{
		var asNode = JsonSerializer.SerializeToNode(schema);

		var locations = GetSubschemaLocations(schema);

		foreach (var location in locations)
		{
			if (!location.TryEvaluate(asNode, out var target))
			{
				// TODO: Actually do something
				continue;
			}

			foreach (var rule in JsonSchemaAnalyzerRules.DefinedRules)
			{
				var diagnostics = rule.Run(target!);
				foreach (var diagnostic in diagnostics)
				{
					diagnostic.RuleId = rule.Id;
					diagnostic.Location = location;
					yield return diagnostic;
				}
			}
		}
	}

	private static IEnumerable<JsonPointer> GetSubschemaLocations(JsonSchema schema)
	{
		yield return JsonPointer.Empty;

		if (schema.BoolValue.HasValue) yield break;

		foreach (var keyword in schema.Keywords!)
		{
			JsonPointer local;
			var keywordName = keyword.Keyword();
			switch (keyword)
			{
				case ISchemaContainer { Schema: { } } container:
					local = JsonPointer.Create(keywordName);
					foreach (var location in GetSubschemaLocations(container.Schema))
					{
						yield return local.Combine(location);
					}
					break;
				case ISchemaCollector collector:
					int i = 0;
					foreach (var subschema in collector.Schemas)
					{
						local = JsonPointer.Create(keywordName, i);
						foreach (var location in GetSubschemaLocations(subschema))
						{
							yield return local.Combine(location);
						}
						i++;
					}
					break;
				case IKeyedSchemaCollector collector:
					foreach (var subschema in collector.Schemas)
					{
						local = JsonPointer.Create(keywordName, subschema.Key);
						foreach (var location in GetSubschemaLocations(subschema.Value))
						{
							yield return local.Combine(location);
						}
					}
					break;
				case ICustomSchemaCollector collector:
					foreach (var subschema in collector.Schemas)
					{
						local = JsonPointer.Create(keywordName);
						foreach (var location in GetSubschemaLocations(subschema))
						{
							yield return local.Combine(location);
						}
					}
					break;
			}
		}
	}

}