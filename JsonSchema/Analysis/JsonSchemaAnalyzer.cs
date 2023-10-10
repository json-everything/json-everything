using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Analysis;

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

			foreach (var rule in GetRules())
			{
				var diagnostics = rule.Run(target!);
				foreach (var diagnostic in diagnostics)
				{
					diagnostic.Location = location.Combine(diagnostic.Location);
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
					foreach (var subschema in collector.Schemas)
					{
						local = JsonPointer.Create(keywordName);
						foreach (var location in GetSubschemaLocations(subschema))
						{
							yield return local.Combine(location);
						}
					}
					break;
				case IKeyedSchemaCollector collector:
					foreach (var subschema in collector.Schemas.Values)
					{
						local = JsonPointer.Create(keywordName);
						foreach (var location in GetSubschemaLocations(subschema))
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

	private static readonly IRule[] _definedRules =
		typeof(JsonSchemaAnalyzerRules)
			.GetFields()
			.Where(x => x.IsPublic && x.FieldType == typeof(IRule))
			.Select(x => (IRule)x.GetValue(null))
			.ToArray();

	private static IEnumerable<IRule> GetRules()
	{
		return _definedRules;
	}
}