using System.Text.Encodings.Web;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests;

[SetUpFixture]
public class TestEnvironment
{
	public static readonly JsonSerializerOptions SerializerOptions =
		new JsonSerializerOptions
		{
			TypeInfoResolverChain = { TestSerializerContext.Default },
		}.WithJsonSchema();

	public static readonly JsonSerializerOptions TestOutputSerializerOptions =
		new JsonSerializerOptions
		{
			TypeInfoResolverChain = { TestSerializerContext.Default },
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		}.WithJsonSchema();

	public static readonly JsonSerializerOptions TestSuiteSerializationOptions =
		new JsonSerializerOptions
		{
			TypeInfoResolverChain = { TestSerializerContext.Default },
			PropertyNameCaseInsensitive = true
		}.WithJsonSchema();


	[OneTimeSetUp]
	public void Setup()
	{
	}
}
