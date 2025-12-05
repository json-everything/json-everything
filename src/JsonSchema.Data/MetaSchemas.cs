using System.IO;
using System.Text;
using System.Text.Json;

namespace Json.Schema.Data;

/// <summary>
/// Provides access to the data extensions vocabulary meta-schema and methods for registering its components.
/// </summary>
/// <remarks>Use this class to register and retrieve the meta-schema required for working with the data
/// vocabulary in JSON Schema processing. All members are static and intended for application-wide
/// configuration.</remarks>
public static class MetaSchemas
{
	/// <summary>
	/// The array extensions vocabulary meta-schema.
	/// </summary>
	public static JsonSchema Data { get; private set; } = null!;

	/// <summary>
	/// Registers the all components required to use the array extensions vocabulary.
	/// </summary>
	public static void Register(BuildOptions? buildOptions = null)
	{
		buildOptions ??= BuildOptions.Default;

		buildOptions.DialectRegistry.Register(Dialect.Data_202012);
		buildOptions.VocabularyRegistry.Register(Vocabulary.Data);

		Data = LoadMetaSchema("data_2023", buildOptions);
	}

	private static JsonSchema LoadMetaSchema(string resourceName, BuildOptions buildOptions)
	{
		var resourceStream = typeof(MetaSchemas).Assembly.GetManifestResourceStream(@$"Json.Schema.Data.Meta_Schemas.{resourceName}.json");
		using var reader = new StreamReader(resourceStream!, Encoding.UTF8);

		var text = reader.ReadToEnd();
		var json = JsonDocument.Parse(text).RootElement;

		return JsonSchema.Build(json, buildOptions);
	}
}