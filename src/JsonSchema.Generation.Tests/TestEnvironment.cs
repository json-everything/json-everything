using System.Drawing;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Schema.Generation.Tests.Serialization;

namespace Json.Schema.Generation.Tests;

internal static class TestEnvironment
{
	public static readonly JsonSerializerOptions SerializerOptions =
		new()
		{
			TypeInfoResolverChain = { TestSerializerContext.Default },
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};
}

[JsonSerializable(typeof(Point))]
[JsonSerializable(typeof(JsonSchema))]
[JsonSerializable(typeof(EvaluationResults))]
[JsonSerializable(typeof(PropertiesKeyword))]
[JsonSerializable(typeof(DeserializationTests.Foo))]
[JsonSerializable(typeof(DeserializationTests.FooWithSchema))]
[JsonSerializable(typeof(ArrayGenerationTests.EnumTest))]
#if NET9_0_OR_GREATER
[JsonSerializable(typeof(ClientTests.Issue890_Status))]
#endif
public partial class TestSerializerContext : JsonSerializerContext;
