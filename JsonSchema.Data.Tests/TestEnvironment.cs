using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Schema.Data.Tests.Suite;

namespace Json.Schema.Data.Tests;

public class TestEnvironment
{
	public static readonly JsonSerializerOptions SerializerOptions =
		new JsonSerializerOptions
		{
			TypeInfoResolverChain = { DataTestsSerializerContext.Default }
		}.WithDataVocab();
}

[JsonSerializable(typeof(TestCollection))]
[JsonSerializable(typeof(List<TestCollection>))]
internal partial class DataTestsSerializerContext : JsonSerializerContext;