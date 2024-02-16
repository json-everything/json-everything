using System.Collections.Generic;
using System.Text.Json.Nodes;

#pragma warning disable CS8618

namespace Json.Schema.Benchmark.Suite;

public class TestCollection : ITestCollection
{
	public string Description { get; set; }
	public JsonSchema Schema { get; set; }
	// ReSharper disable once CollectionNeverUpdated.Global
	public List<TestCase> Tests { get; set; }
	public bool IsOptional { get; set; }
	public EvaluationOptions Options { get; set; }
}

public class TestCollectionFunctional : ITestCollection
{
	public string Description { get; set; }
	public JsonNode? Schema { get; set; }
	// ReSharper disable once CollectionNeverUpdated.Global
	public List<TestCase> Tests { get; set; }
	public bool IsOptional { get; set; }
	public EvaluationOptions Options { get; set; }
}

public interface ITestCollection
{
	public string Description { get; set; }
	// ReSharper disable once CollectionNeverUpdated.Global
	public List<TestCase> Tests { get; set; }
	public bool IsOptional { get; set; }
	public EvaluationOptions Options { get; set; }
}