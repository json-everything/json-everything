using System.Collections.Generic;
#pragma warning disable CS8618

namespace Json.Schema.Tests.Suite;

/// <summary>
/// The root of a unified test suite file: a description and the test cases it contains.
/// </summary>
public class TestSuiteFile
{
	public string Description { get; set; }
	// ReSharper disable once CollectionNeverUpdated.Global
	public List<TestCollection> Tests { get; set; }
}
