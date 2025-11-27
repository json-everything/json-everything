using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Json.Schema;

public static class MetaSchemas
{
	public static readonly Uri Draft6Id = new("http://json-schema.org/draft-06/schema#");
	public static readonly JsonSchema Draft6;

	public static readonly Uri Draft7Id = new("http://json-schema.org/draft-07/schema#");
	public static readonly JsonSchema Draft7;

	public static readonly Uri Draft201909Id = new("https://json-schema.org/draft/2019-09/schema");
	public static readonly JsonSchema Applicator201909;
	public static readonly JsonSchema Content201909;
	public static readonly JsonSchema Core201909;
	public static readonly JsonSchema Format201909;
	public static readonly JsonSchema Metadata201909;
	public static readonly JsonSchema Draft201909;
	public static readonly JsonSchema Validation201909;

	public static readonly Uri Draft202012Id = new("https://json-schema.org/draft/2020-12/schema");
	public static readonly JsonSchema Applicator202012;
	public static readonly JsonSchema Content202012;
	public static readonly JsonSchema Core202012;
	public static readonly JsonSchema FormatAnnotation202012;
	public static readonly JsonSchema FormatAssertion202012;
	public static readonly JsonSchema Metadata202012;
	public static readonly JsonSchema Draft202012;
	public static readonly JsonSchema Unevaluated202012;
	public static readonly JsonSchema Validation202012;

	public static readonly Uri V1Id = new("https://json-schema.org/draft/next/schema");
	public static readonly JsonSchema V1;

	static MetaSchemas()
	{
		Draft6 = Load("schema06");
		Draft7 = Load("schema07");
		Applicator201909 = Load("_2019_09.applicator");
		Content201909 = Load("_2019_09.content");
		Core201909 = Load("_2019_09.core");
		Format201909 = Load("_2019_09.format");
		Metadata201909 = Load("_2019_09.meta-data");
		Draft201909 = Load("_2019_09.schema");
		Validation201909 = Load("_2019_09.validation");
		Applicator202012 = Load("_2020_12.applicator");
		Content202012 = Load("_2020_12.content");
		Core202012 = Load("_2020_12.core");
		FormatAnnotation202012 = Load("_2020_12.format-annotation");
		FormatAssertion202012 = Load("_2020_12.format-assertion");
		Metadata202012 = Load("_2020_12.meta-data");
		Draft202012 = Load("_2020_12.schema");
		Unevaluated202012 = Load("_2020_12.unevaluated");
		Validation202012 = Load("_2020_12.validation");
		V1 = Load("v1");

		SchemaRegistry.Global.Register(new Uri("https://json-schema.org/v1", UriKind.Absolute), V1);
	}

	private static JsonSchema Load(string resourceName)
	{
		var resourceStream = typeof(MetaSchemas).Assembly.GetManifestResourceStream(@$"Json.Schema.Meta_Schemas.{resourceName}.json");
		using var reader = new StreamReader(resourceStream!, Encoding.UTF8);

		var text = reader.ReadToEnd();
		var json = JsonDocument.Parse(text).RootElement;

		return JsonSchema.Build(json);
	}
}