using System.Collections.Generic;
using System.Text.Json.Nodes;

#pragma warning disable CS8618

namespace Json.Schema.Tests.Suite;

public class TestCollection
{
	public string Description { get; set; }
	public JsonSchema Schema { get; set; }
	// ReSharper disable once CollectionNeverUpdated.Global
	public List<TestCase> Tests { get; set; }
	public bool IsOptional { get; set; }
}

public class TestCollectionFunctional
{
	public string Description { get; set; }
	public JsonNode? Schema { get; set; }
	// ReSharper disable once CollectionNeverUpdated.Global
	public List<TestCase> Tests { get; set; }
	public bool IsOptional { get; set; }
}