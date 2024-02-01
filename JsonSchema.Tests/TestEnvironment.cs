using System.Text.Encodings.Web;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests;

[SetUpFixture]
public class TestEnvironment
{
	public static readonly JsonSerializerOptions SerializerOptions = new()
	{
		TypeInfoResolverChain = { TestSerializerContext.Default, JsonSchemaSerializerContext.Default },
	};

	public static readonly JsonSerializerOptions SerializerOptionsUnsafeRelaxedEscaping = new(SerializerOptions)
	{
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

	public static readonly JsonSerializerOptions TestSuiteSerializationOptions = new(SerializerOptions)
	{
		PropertyNameCaseInsensitive = true
	};


	[OneTimeSetUp]
	public void Setup()
	{
	}
}
