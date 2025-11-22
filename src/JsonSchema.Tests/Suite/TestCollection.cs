using System.Collections.Generic;
using System.Text.Json;
#pragma warning disable CS8618

namespace Json.Schema.Tests.Suite;

public class TestCollection
{
	public string Description { get; set; }
	public JsonElement Schema { get; set; }
	// ReSharper disable once CollectionNeverUpdated.Global
	public List<TestCase> Tests { get; set; }
	public bool IsOptional { get; set; }
}