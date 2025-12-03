using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Json.Schema.ArrayExt;

/// <summary>
/// Defines a meta-schema for the 
/// </summary>
public static class MetaSchemas
{
	/// <summary>
	/// The ID for the draft 2020-12 extension vocabulary which includes the array extensions vocabulary.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static Uri ArrayExt_202012Id { get; } = new("https://json-everything.net/meta/array-ext");

	/// <summary>
	/// The array extensions vocabulary meta-schema.
	/// </summary>
	public static JsonSchema ArrayExt { get; private set; }

	/// <summary>
	/// Registers the all components required to use the array extensions vocabulary.
	/// </summary>
	public static void Register(BuildOptions? buildOptions = null)
	{
		buildOptions ??= BuildOptions.Default;

		ArrayExt = LoadMetaSchema("array-ext", buildOptions);
	}

	private static JsonSchema LoadMetaSchema(string resourceName, BuildOptions buildOptions)
	{
		var resourceStream = typeof(MetaSchemas).Assembly.GetManifestResourceStream(@$"Json.Schema.Meta_Schemas.{resourceName}.json");
		using var reader = new StreamReader(resourceStream!, Encoding.UTF8);

		var text = reader.ReadToEnd();
		var json = JsonDocument.Parse(text).RootElement;

		return JsonSchema.Build(json, buildOptions);
	}
}