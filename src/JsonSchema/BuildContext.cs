using System;
using System.Text.Json;

namespace Json.Schema;

public struct BuildContext
{
	public BuildOptions Options { get; }
	public JsonElement RootSchema { get; }
	public Uri BaseUri { get; set; }
	public JsonElement LocalSchema { get; set; }
	public Dialect Dialect { get; set; }

	internal BuildContext(BuildOptions options, JsonElement rootSchema, Uri baseUri)
	{
		Options = options;
		RootSchema = rootSchema;
		BaseUri = baseUri;
		Dialect = Options.Dialect;
	}
}