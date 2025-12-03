using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
// ReSharper disable InconsistentNaming
#pragma warning disable IDE1006

namespace Json.Schema;

internal static class __ModuleInitialization
{
	private static int _initialized;

	[ModuleInitializer]
	public static void Initialize()
	{
		if (System.Threading.Interlocked.Exchange(ref _initialized, 1) == 1) return;

		LoadDialects();

		LoadMetaSchemas();
	}

	private static void LoadDialects()
	{
		VocabularyRegistry.Global.RegisterDefaultVocabs();
		DialectRegistry.Global.RegisterDefaultDialects();

		Dialect.Default = Dialect.V1;
	}

	private static void LoadMetaSchemas()
	{
		var buildOptions = new BuildOptions();

		MetaSchemas.Draft6 = LoadMetaSchema("schema06", buildOptions);

		MetaSchemas.Draft7 = LoadMetaSchema("schema07", buildOptions);

		MetaSchemas.Applicator201909 = LoadMetaSchema("_2019_09.applicator", buildOptions);
		MetaSchemas.Content201909 = LoadMetaSchema("_2019_09.content", buildOptions);
		MetaSchemas.Core201909 = LoadMetaSchema("_2019_09.core", buildOptions);
		MetaSchemas.Format201909 = LoadMetaSchema("_2019_09.format", buildOptions);
		MetaSchemas.Metadata201909 = LoadMetaSchema("_2019_09.meta-data", buildOptions);
		MetaSchemas.Validation201909 = LoadMetaSchema("_2019_09.validation", buildOptions);
		MetaSchemas.Draft201909 = LoadMetaSchema("_2019_09.schema", buildOptions);

		MetaSchemas.Applicator202012 = LoadMetaSchema("_2020_12.applicator", buildOptions);
		MetaSchemas.Content202012 = LoadMetaSchema("_2020_12.content", buildOptions);
		MetaSchemas.Core202012 = LoadMetaSchema("_2020_12.core", buildOptions);
		MetaSchemas.FormatAnnotation202012 = LoadMetaSchema("_2020_12.format-annotation", buildOptions);
		MetaSchemas.FormatAssertion202012 = LoadMetaSchema("_2020_12.format-assertion", buildOptions);
		MetaSchemas.Metadata202012 = LoadMetaSchema("_2020_12.meta-data", buildOptions);
		MetaSchemas.Unevaluated202012 = LoadMetaSchema("_2020_12.unevaluated", buildOptions);
		MetaSchemas.Validation202012 = LoadMetaSchema("_2020_12.validation", buildOptions);
		MetaSchemas.Draft202012 = LoadMetaSchema("_2020_12.schema", buildOptions);
		
		MetaSchemas.V1_2026 = LoadMetaSchema("v1", buildOptions);

		SchemaRegistry.Global.Register(new Uri("https://json-schema.org/v1", UriKind.Absolute), MetaSchemas.V1_2026);
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