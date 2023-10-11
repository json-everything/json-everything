using System.Linq;
using System.Reflection;

namespace Json.Schema.Analysis;

public static partial class JsonSchemaAnalyzerRules
{
	internal static readonly JsonSchema[] DefinedMetaSchemas;
	internal static readonly IRule[] DefinedRules;

	static JsonSchemaAnalyzerRules()
	{
		DefinedMetaSchemas = typeof(JsonSchemaAnalyzerRules)
			.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			.Where(x => x.FieldType == typeof(JsonSchema))
			.Select(x => (JsonSchema)x.GetValue(null))
			.ToArray();
		DefinedRules = typeof(JsonSchemaAnalyzerRules)
			.GetFields(BindingFlags.Static | BindingFlags.Public)
			.Where(x => x.IsPublic && x.FieldType == typeof(JsonSchema))
			.Select(x => (IRule)new MetaSchemaRule((JsonSchema)x.GetValue(null)))
			.ToArray();
	}
}