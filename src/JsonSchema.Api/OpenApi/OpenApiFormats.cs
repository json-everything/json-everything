using System;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// The formats a description can be served in.
/// </summary>
[Flags]
public enum OpenApiFormats
{
	/// <summary>
	/// Served as JSON, at the document path with a `.json` extension.
	/// </summary>
	Json = 1,
	/// <summary>
	/// Served as YAML, at the document path with a `.yaml` or `.yml` extension.
	/// </summary>
	Yaml = 2
}