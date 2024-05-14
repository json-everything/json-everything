using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;

namespace Json.Schema.OpenApi.Tests;

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
		Vocabularies.Register();
		EvaluationOptions.Default.OutputFormat = OutputFormat.Hierarchical;
	}
}

[JsonSerializable(typeof(JsonSchema))]
[JsonSerializable(typeof(EvaluationResults))]
internal partial class TestSerializationContext : JsonSerializerContext;