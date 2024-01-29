using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Json.Schema.Tests.Suite;
using NUnit.Framework;

namespace Json.Schema.Tests;

[SetUpFixture]
public class TestEnvironment
{
	public static JsonSerializerOptions SerializerOptions = new()
	{
#if NET8_0_OR_GREATER
		TypeInfoResolverChain = { TestSerializerContext.Default, JsonSchema.TypeInfoResolver },
#endif
	};

	public static JsonSerializerOptions SerializerOptionsUnsafeRelaxedEscaping = new(SerializerOptions)
	{
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

[OneTimeSetUp]
	public void Setup()
	{
	}
}
