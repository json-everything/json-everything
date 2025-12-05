using System.IO;
using System.Text;
using System.Text.Json;

namespace Json.Schema.ArrayExt;

/// <summary>
/// Provides access to meta-schemas and registration methods for the array extensions vocabulary used in JSON Schema
/// processing.
/// </summary>
/// <remarks>Use this class to register and retrieve the meta-schema required for validating schemas that utilize
/// the array extensions vocabulary. All members are static and intended for application-wide configuration.</remarks>
public static class MetaSchemas
{
	/// <summary>
	/// The array extensions vocabulary meta-schema.
	/// </summary>
	public static JsonSchema ArrayExt { get; private set; } = null!;

	/// <summary>
	/// Registers the all components required to use the array extensions vocabulary.
	/// </summary>
	public static void Register(BuildOptions? buildOptions = null)
	{
		buildOptions ??= BuildOptions.Default;

		buildOptions.DialectRegistry.Register(Dialect.ArrayExt_202012);
		buildOptions.VocabularyRegistry.Register(Vocabulary.ArrayExt);

		ArrayExt = LoadMetaSchema("array-ext", buildOptions);
	}

	private static JsonSchema LoadMetaSchema(string resourceName, BuildOptions buildOptions)
	{
		var resourceStream = typeof(MetaSchemas).Assembly.GetManifestResourceStream(@$"Json.Schema.ArrayExt.Meta_Schemas.{resourceName}.json");
		using var reader = new StreamReader(resourceStream!, Encoding.UTF8);

		var text = reader.ReadToEnd();
		var json = JsonDocument.Parse(text).RootElement;

		return JsonSchema.Build(json, buildOptions);
	}
}