using System;

namespace Json.Schema.Tests.Suite;

public static class TestCollectionExtensions
{
	/// <summary>
	/// Builds the test case's external schemas and registers them at their retrieval URIs
	/// in the registry configured on <paramref name="buildOptions"/>.
	/// </summary>
	public static void RegisterExternalSchemas(this TestCollection collection, BuildOptions buildOptions)
	{
		if (collection.ExternalSchemas is null) return;

		foreach (var externalSchema in collection.ExternalSchemas)
		{
			var uri = new Uri(externalSchema.Key);
			var schema = JsonSchema.Build(externalSchema.Value, buildOptions, uri);
			buildOptions.SchemaRegistry.Register(uri, schema);
		}
	}
}
