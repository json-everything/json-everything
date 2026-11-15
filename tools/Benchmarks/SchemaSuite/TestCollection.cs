using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Schema;

#pragma warning disable CS8618

namespace Json.Benchmarks.SchemaSuite;

public class TestCollection
{
	public string Description { get; set; }
	public string? Compatibility { get; set; }
	public List<string>? Tags { get; set; }
	public JsonElement Schema { get; set; }
	public Dictionary<string, JsonElement>? ExternalSchemas { get; set; }
	// ReSharper disable once CollectionNeverUpdated.Global
	public List<TestCase> Tests { get; set; }
	public bool IsOptional { get; set; }
	[JsonIgnore]
	public Dialect Dialect { get; set; }
	[JsonIgnore]
	public EvaluationOptions EvaluationOptions { get; set; }

	/// <summary>
	/// Creates build options with a fresh registry that contains the test case's external schemas
	/// registered at their retrieval URIs.
	/// </summary>
	public BuildOptions CreateBuildOptions()
	{
		var buildOptions = new BuildOptions
		{
			Dialect = Dialect,
			SchemaRegistry = new()
		};

		if (ExternalSchemas is null) return buildOptions;

		foreach (var externalSchema in ExternalSchemas)
		{
			var uri = new Uri(externalSchema.Key);
			var schema = JsonSchema.Build(externalSchema.Value, buildOptions, uri);
			buildOptions.SchemaRegistry.Register(uri, schema);
		}

		return buildOptions;
	}
}
