using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Logic.Tests.Suite;
using NUnit.Framework;

namespace Json.Logic.Tests;

[SetUpFixture]
public class TestEnvironment
{
	public static readonly JsonSerializerOptions SerializerOptions =
		new()
		{
			TypeInfoResolverChain = { TestSerializerContext.Default },
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};


	[OneTimeSetUp]
	public void Setup()
	{
	}
}

[JsonSerializable(typeof(TestSuite))]
[JsonSerializable(typeof(Test))]
[JsonSerializable(typeof(Test[]))]
[JsonSerializable(typeof(Rule))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(JsonNode))]
internal partial class TestSerializerContext : JsonSerializerContext;