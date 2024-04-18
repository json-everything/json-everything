using System.Collections.Generic;
using Json.Schema;

#pragma warning disable CS8618

namespace Json.Benchmarks.SchemaSuite;

public class TestCollection
{
	public string Description { get; set; }
	public JsonSchema Schema { get; set; }
	// ReSharper disable once CollectionNeverUpdated.Global
	public List<TestCase> Tests { get; set; }
	public bool IsOptional { get; set; }
	public EvaluationOptions Options { get; set; }
}