using System;
// ReSharper disable InconsistentNaming
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Json.Schema;

public static class MetaSchemas
{
	public static Uri Draft6Id { get; } = new("http://json-schema.org/draft-06/schema#");
	public static JsonSchema Draft6 { get; internal set; }

	public static Uri Draft7Id { get; } = new("http://json-schema.org/draft-07/schema#");
	public static JsonSchema Draft7 { get; internal set; }

	public static Uri Draft201909Id { get; } = new("https://json-schema.org/draft/2019-09/schema");
	public static JsonSchema Applicator201909 { get; internal set; }
	public static JsonSchema Content201909 { get; internal set; }
	public static JsonSchema Core201909 { get; internal set; }
	public static JsonSchema Format201909 { get; internal set; }
	public static JsonSchema Metadata201909 { get; internal set; }
	public static JsonSchema Draft201909 { get; internal set; }
	public static JsonSchema Validation201909 { get; internal set; }

	public static Uri Draft202012Id { get; } = new("https://json-schema.org/draft/2020-12/schema");
	public static JsonSchema Applicator202012 { get; internal set; }
	public static JsonSchema Content202012 { get; internal set; }
	public static JsonSchema Core202012 { get; internal set; }
	public static JsonSchema FormatAnnotation202012 { get; internal set; }
	public static JsonSchema FormatAssertion202012 { get; internal set; }
	public static JsonSchema Metadata202012 { get; internal set; }
	public static JsonSchema Draft202012 { get; internal set; }
	public static JsonSchema Unevaluated202012 { get; internal set; }
	public static JsonSchema Validation202012 { get; internal set; }

	public static Uri V1Id { get; } = new("https://json-schema.org/v1");
	public static Uri V1_2026Id { get; } = new("https://json-schema.org/v1/2026");
	public static JsonSchema V1_2026 { get; internal set; }
}