using System.Collections.Generic;

#pragma warning disable CS8618

namespace Json.Benchmarks.SchemaSuite;

public class TestFile
{
	public string Description { get; set; }
	// ReSharper disable once CollectionNeverUpdated.Global
	public List<TestCollection> Tests { get; set; }
}
