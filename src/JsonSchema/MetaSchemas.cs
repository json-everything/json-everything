using System;
// ReSharper disable InconsistentNaming
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Json.Schema;

/// <summary>
/// Provides access to well-known JSON Schema meta-schemas and their identifiers for supported specification drafts.
/// </summary>
/// <remarks>This class exposes static properties representing the canonical URIs and parsed schema objects for
/// various JSON Schema drafts, including Draft-06, Draft-07, Draft 2019-09, Draft 2020-12, the v1 releases.</remarks>
public static class MetaSchemas
{
	/// <summary>
	/// Gets the canonical URI identifying the JSON Schema Draft 6 specification.
	/// </summary>
	public static Uri Draft6Id { get; } = new("http://json-schema.org/draft-06/schema#");
	/// <summary>
	/// Gets the meta-schema for the JSON Schema Draft 6 specification.
	/// </summary>
	public static JsonSchema Draft6 { get; internal set; }

	/// <summary>
	/// Gets the canonical URI identifying the JSON Schema Draft 7 specification.
	/// </summary>
	public static Uri Draft7Id { get; } = new("http://json-schema.org/draft-07/schema#");
	/// <summary>
	/// Gets the meta-schema for the JSON Schema Draft 7 specification.
	/// </summary>
	public static JsonSchema Draft7 { get; internal set; }

	/// <summary>
	/// Gets the canonical URI identifying the JSON Schema Draft 2019-09 meta-schema.
	/// </summary>
	public static Uri Draft201909Id { get; } = new("https://json-schema.org/draft/2019-09/schema");
	/// <summary>
	/// Gets the meta-schema for the JSON Schema Draft 2019-09 specification.
	/// </summary>
	public static JsonSchema Draft201909 { get; internal set; }
	/// <summary>
	/// Gets the canonical URI identifying the Applicator meta-schema for the JSON Schema Draft 2019-09 specification.
	/// </summary>
	public static Uri Applicator201909Id { get; } = new("https://json-schema.org/draft/2019-09/meta/applicator");
	/// <summary>
	/// Gets the Applicator meta-schema for the JSON Schema Draft 2019-09 specification.
	/// </summary>
	public static JsonSchema Applicator201909 { get; internal set; }
	/// <summary>
	/// Gets the canonical URI identifying the Content meta-schema for the JSON Schema Draft 2019-09 specification.
	/// </summary>
	public static Uri Content201909Id { get; } = new("https://json-schema.org/draft/2019-09/meta/content");
	/// <summary>
	/// Gets the Content meta-schema for the JSON Schema Draft 2019-09 specification.
	/// </summary>
	public static JsonSchema Content201909 { get; internal set; }
	/// <summary>
	/// Gets the canonical URI identifying the Core meta-schema for the JSON Schema Draft 2019-09 specification.
	/// </summary>
	public static Uri Core201909Id { get; } = new("https://json-schema.org/draft/2019-09/meta/core");
	/// <summary>
	/// Gets the Core meta-schema for the JSON Schema Draft 2019-09 specification.
	/// </summary>
	public static JsonSchema Core201909 { get; internal set; }
	/// <summary>
	/// Gets the canonical URI identifying the Format meta-schema for the JSON Schema Draft 2019-09 specification.
	/// </summary>
	public static Uri Format201909Id { get; } = new("https://json-schema.org/draft/2019-09/meta/format");
	/// <summary>
	/// Gets the Format meta-schema for the JSON Schema Draft 2019-09 specification.
	/// </summary>
	public static JsonSchema Format201909 { get; internal set; }
	/// <summary>
	/// Gets the canonical URI identifying the Meta-Data meta-schema for the JSON Schema Draft 2019-09 specification.
	/// </summary>
	public static Uri Metadata201909Id { get; } = new("https://json-schema.org/draft/2019-09/meta/meta-data");
	/// <summary>
	/// Gets the Meta-Data meta-schema for the JSON Schema Draft 2019-09 specification.
	/// </summary>
	public static JsonSchema Metadata201909 { get; internal set; }
	/// <summary>
	/// Gets the canonical URI identifying the Validation meta-schema for the JSON Schema Draft 2019-09 specification.
	/// </summary>
	public static Uri Validation201909Id { get; } = new("https://json-schema.org/draft/2019-09/meta/validation");
	/// <summary>
	/// Gets the Validation meta-schema for the JSON Schema Draft 2019-09 specification.
	/// </summary>
	public static JsonSchema Validation201909 { get; internal set; }

	/// <summary>
	/// Gets the canonical URI identifying the JSON Schema Draft 2020-12 meta-schema.
	/// </summary>
	public static Uri Draft202012Id { get; } = new("https://json-schema.org/draft/2020-12/schema");
	/// <summary>
	/// Gets the meta-schema for the JSON Schema Draft 2020-12 specification.
	/// </summary>
	public static JsonSchema Draft202012 { get; internal set; }
	/// <summary>
	/// Gets the canonical URI identifying the Applicator meta-schema for the JSON Schema Draft 2020-12 specification.
	/// </summary>
	public static Uri Applicator202012Id { get; } = new("https://json-schema.org/draft/2020-12/meta/applicator");
	/// <summary>
	/// Gets the Applicator meta-schema for the JSON Schema Draft 2020-12 specification.
	/// </summary>
	public static JsonSchema Applicator202012 { get; internal set; }
	/// <summary>
	/// Gets the canonical URI identifying the Content meta-schema for the JSON Schema Draft 2020-12 specification.
	/// </summary>
	public static Uri Content202012Id { get; } = new("https://json-schema.org/draft/2020-12/meta/content");
	/// <summary>
	/// Gets the Content meta-schema for the JSON Schema Draft 2020-12 specification.
	/// </summary>
	public static JsonSchema Content202012 { get; internal set; }
	/// <summary>
	/// Gets the canonical URI identifying the Core meta-schema for the JSON Schema Draft 2020-12 specification.
	/// </summary>
	public static Uri Core202012Id { get; } = new("https://json-schema.org/draft/2020-12/meta/core");
	/// <summary>
	/// Gets the Core meta-schema for the JSON Schema Draft 2020-12 specification.
	/// </summary>
	public static JsonSchema Core202012 { get; internal set; }
	/// <summary>
	/// Gets the canonical URI identifying the Format-Annotation meta-schema for the JSON Schema Draft 2020-12 specification.
	/// </summary>
	public static Uri FormatAnnotation202012Id { get; } = new("https://json-schema.org/draft/2020-12/meta/format-annotation");
	/// <summary>
	/// Gets the Format-Annotation meta-schema for the JSON Schema Draft 2020-12 specification.
	/// </summary>
	public static JsonSchema FormatAnnotation202012 { get; internal set; }
	/// <summary>
	/// Gets the canonical URI identifying the Format-Assertion meta-schema for the JSON Schema Draft 2020-12 specification.
	/// </summary>
	public static Uri FormatAssertion202012Id { get; } = new("https://json-schema.org/draft/2020-12/meta/format-assertion");
	/// <summary>
	/// Gets the Format-Assertion meta-schema for the JSON Schema Draft 2020-12 specification.
	/// </summary>
	public static JsonSchema FormatAssertion202012 { get; internal set; }
	/// <summary>
	/// Gets the canonical URI identifying the Meta-Data meta-schema for the JSON Schema Draft 2020-12 specification.
	/// </summary>
	public static Uri Metadata202012Id { get; } = new("https://json-schema.org/draft/2020-12/meta/meta-data");
	/// <summary>
	/// Gets the Meta-Data meta-schema for the JSON Schema Draft 2020-12 specification.
	/// </summary>
	public static JsonSchema Metadata202012 { get; internal set; }
	/// <summary>
	/// Gets the canonical URI identifying the Unevaluated meta-schema for the JSON Schema Draft 2020-12 specification.
	/// </summary>
	public static Uri Unevaluated202012Id { get; } = new("https://json-schema.org/draft/2020-12/meta/unevaluated");
	/// <summary>
	/// Gets the Unevaluated meta-schema for the JSON Schema Draft 2020-12 specification.
	/// </summary>
	public static JsonSchema Unevaluated202012 { get; internal set; }
	/// <summary>
	/// Gets the canonical URI identifying the Validation meta-schema for the JSON Schema Draft 2020-12 specification.
	/// </summary>
	public static Uri Validation202012Id { get; } = new("https://json-schema.org/draft/2020-12/meta/validation");
	/// <summary>
	/// Gets the Validation meta-schema for the JSON Schema Draft 2020-12 specification.
	/// </summary>
	public static JsonSchema Validation202012 { get; internal set; }

	/// <summary>
	/// Gets the URI that identifies version 1 of the JSON Schema specification.
	/// </summary>
	public static Uri V1Id { get; } = new("https://json-schema.org/v1");
	/// <summary>
	/// Gets the canonical URI identifying the JSON Schema version v1, release 2026 specification.
	/// </summary>
	public static Uri V1_2026Id { get; } = new("https://json-schema.org/v1/2026");
	/// <summary>
	/// Gets the meta-schema for the JSON Schema version 1, release 2026 specification.
	/// </summary>
	public static JsonSchema V1_2026 { get; internal set; }
}