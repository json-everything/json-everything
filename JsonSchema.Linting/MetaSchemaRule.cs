using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Schema.Linting;

public class MetaSchemaRule : IRule
{

	public string Id { get; }
	public JsonSchema MetaSchema { get; }

	static MetaSchemaRule()
	{
	}

	public MetaSchemaRule(JsonSchema metaSchema)
	{
		MetaSchema = metaSchema;
		Id = metaSchema.BaseUri.OriginalString;
	}

	public IEnumerable<Diagnostic> Run(JsonNode schema)
	{
		var validation = MetaSchema.Evaluate(schema, JsonSchemaAnalyzerRules.EvaluationOptions);

		var diagnostics = FindDiagnostics(validation);

		return diagnostics;
	}

	private static IEnumerable<Diagnostic> FindDiagnostics(EvaluationResults root)
	{
		if (!root.IsValid) yield break;

		if (root.HasAnnotations &&
		    root.TryGetAnnotation(Diagnostic.MessageKeyword, out var message))
		{
			var target = root.TryGetAnnotation(Diagnostic.TargetKeyword, out var targetAnnotation)
				? targetAnnotation!.GetValue<string>()
				: null;
			yield return new Diagnostic
			{
				Target = target,
				Message = message!.GetValue<string>()
			};
		}

		if (!root.HasDetails) yield break;

		foreach (var detail in root.Details)
		{
			var results = FindDiagnostics(detail);
			foreach (var result in results)
			{
				yield return result;
			}
		}
	}
}