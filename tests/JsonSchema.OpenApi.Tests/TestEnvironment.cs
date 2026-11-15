using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using NUnit.Framework;

namespace Json.Schema.OpenApi.Tests;

[SetUpFixture]
public class TestEnvironment
{
	public static readonly JsonSerializerOptions SerializerOptions =
		new()
		{
			TypeInfoResolverChain = { TestSerializationContext.Default },
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};

	[OneTimeSetUp]
	public void Setup()
	{
		BuildOptions.Default.Dialect = Dialect.OpenApi_31;
		MetaSchemas.Register();
		EvaluationOptions.Default.OutputFormat = OutputFormat.Hierarchical;
	}
}

[JsonSerializable(typeof(JsonNode))]
internal partial class TestSerializationContext : JsonSerializerContext;