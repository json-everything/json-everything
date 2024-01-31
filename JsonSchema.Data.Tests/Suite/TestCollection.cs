using System.Collections.Generic;
using System.Text.Json.Serialization;
using Json.More;
#pragma warning disable CS8618

namespace Json.Schema.Data.Tests.Suite;

public class TestCollection
{
	public string Description { get; set; }
	public JsonSchema Schema { get; set; }
	// ReSharper disable once CollectionNeverUpdated.Global
	public List<TestCase> Tests { get; set; }
	public bool IsOptional { get; set; }
}

[JsonSerializable(typeof(TestCollection))]
[JsonSerializable(typeof(List<TestCollection>))]
public partial class DataTestsSerializerContext : JsonSerializerContext
{
	public static TypeResolverOptionsManager OptionsManager { get; }

	static DataTestsSerializerContext()
	{
		OptionsManager = new(
			Default,
			Json.Schema.JsonSchema.TypeInfoResolver
		);
	}
}