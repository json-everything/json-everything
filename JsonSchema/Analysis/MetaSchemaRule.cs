using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema.Analysis;

public class MetaSchemaRule : IRule
{
	private static readonly EvaluationOptions _options = new()
	{
		OutputFormat = OutputFormat.Hierarchical
	};

	public JsonSchema MetaSchema { get; set; }

	public IEnumerable<Diagnostic> Run(JsonNode schema)
	{
		var validation = MetaSchema.Evaluate(schema, _options);

		var allAnnotations = FindDiagnostics(validation);

		return allAnnotations;
	}

	private static IEnumerable<Diagnostic> FindDiagnostics(EvaluationResults root)
	{
		if (!root.IsValid) yield break;

		if (root.HasAnnotations &&
		    root.TryGetAnnotation("x-diagnostic-message", out var message))
			yield return new Diagnostic
			{
				Location = JsonPointer.Empty,
				Message = message!.GetValue<string>()
			};

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