using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Schema.Data.Tests.Suite;
using NUnit.Framework;

namespace Json.Schema.Data.Tests;

[SetUpFixture]
public class TestEnvironment
{
	public static readonly JsonSerializerOptions SerializerOptions =
		new()
		{
			TypeInfoResolverChain = { DataTestsSerializerContext.Default },
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};

	[OneTimeSetUp]
	public void Setup()
	{
		BuildOptions.Default.Dialect = Dialect.Data_202012;
		MetaSchemas.Register();
		EvaluationOptions.Default.OutputFormat = OutputFormat.Hierarchical;
	}
}

[JsonSerializable(typeof(TestCollection))]
[JsonSerializable(typeof(List<TestCollection>))]
[JsonSerializable(typeof(JsonSchema))]
[JsonSerializable(typeof(EvaluationResults))]
internal partial class DataTestsSerializerContext : JsonSerializerContext;