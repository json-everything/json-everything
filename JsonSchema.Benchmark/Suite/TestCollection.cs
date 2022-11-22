using System.Collections.Generic;

#pragma warning disable CS8618

namespace Json.Schema.Benchmark.Suite;

public class TestCollection
{
	public string Filename { get; set; }
	public JsonSchema Schema { get; set; }
	// ReSharper disable once CollectionNeverUpdated.Global
	public List<TestCase> Tests { get; set; }
	public bool IsOptional { get; set; }
	public EvaluationOptions Options { get; set; }
}