using System.Text.Encodings.Web;
using System.Text.Json;
using Json.Schema.Tests.Suite;
using NUnit.Framework;

namespace Json.Schema.Tests;

[SetUpFixture]
public class TestEnvironment
{
	public static readonly JsonSerializerOptions SerializerOptions = new()
	{
		TypeInfoResolverChain = { TestSerializerContext.Default, JsonSchema.TypeInfoResolver },
	};

	public static readonly JsonSerializerOptions SerializerOptionsUnsafeRelaxedEscaping = new(SerializerOptions)
	{
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

[OneTimeSetUp]
	public void Setup()
	{
	}
}
