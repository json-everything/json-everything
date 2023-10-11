using System.Linq;
using System.Reflection;

namespace Json.Schema.Linting;

public static partial class JsonSchemaAnalyzerRules
{
	internal static readonly EvaluationOptions EvaluationOptions;

	internal static readonly IRule[] DefinedRules;

	static JsonSchemaAnalyzerRules()
	{
		var definedMetaSchemas = typeof(JsonSchemaAnalyzerRules)
			.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			.Where(x => x.FieldType == typeof(JsonSchema))
			.Select(x => (JsonSchema)x.GetValue(null))
			.ToArray();

		EvaluationOptions = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical
		};
		Data.Vocabularies.Register(EvaluationOptions.VocabularyRegistry, EvaluationOptions.SchemaRegistry);
		foreach (var metaSchema in definedMetaSchemas)
		{
			EvaluationOptions.SchemaRegistry.Register(metaSchema);
		}

		DefinedRules = typeof(JsonSchemaAnalyzerRules)
			.GetFields(BindingFlags.Static | BindingFlags.Public)
			.Where(x => x.IsPublic && x.FieldType == typeof(JsonSchema))
			.Select(x => (IRule)new MetaSchemaRule((JsonSchema)x.GetValue(null)))
			.ToArray();
	}
}